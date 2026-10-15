import { readFileSync, writeFileSync } from 'node:fs';

const model = JSON.parse(readFileSync(new URL('model.json', import.meta.url), 'utf8'));
const unitIap = (model.price_vat_inclusive / (1 + model.vat_rate)
  * (1 - model.iap_market_fee_of_supply)
  - model.price_vat_inclusive * model.iap_toss_fee_of_consumer_price)
  * (1 - model.refund_reserve);

function estimate(dau, newUsers, scenario, adFee = model.planning_ad_toss_fee) {
  const perFreeDay = scenario.exposures.reduce((sum, n, i) => sum + n * scenario.ecpm[i] / 1000, 0);
  const beforeTossFee = dau * model.month_days * (1 - model.average_ad_free_share_of_daily_active_users) * perFreeDay;
  const ads = beforeTossFee * (1 - adFee);
  const buyers = newUsers * scenario.first_purchase_conversion;
  const iap = buyers * unitIap;
  return { average_dau: dau, monthly_new_users: newUsers, first_purchases: buyers, ad_toss_fee: adFee,
    ads_before_toss_fee: beforeTossFee, ads, iap, total: ads + iap };
}

const base = model.scenarios.base;
const result = {
  as_of: model.as_of,
  status: model.status,
  unit_iap_after_refund_reserve: unitIap,
  scale: model.scale_dau.map(d => estimate(d, d * model.new_users_to_dau_ratio_for_comparison_only, base)),
  sensitivity: Object.entries(model.scenarios).map(([name, scenario]) => ({ name, ...estimate(1000, 5000, scenario) })),
  promo: estimate(1000, 5000, base, model.promo_ad_toss_fee),
  targets: model.targets.map(t => ({ ...t, ...estimate(t.average_dau, t.monthly_new_users, base) })),
  ad_only_dau_for_1000000: Math.ceil(1000000 / estimate(1, 0, base).ads),
  excluded: model.excluded
};
writeFileSync(new URL('estimates.json', import.meta.url), JSON.stringify(result, null, 2) + '\n');
console.log('Updated Overview/Revenue/estimates.json');
