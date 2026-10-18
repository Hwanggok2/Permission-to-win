using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace StageGame
{
    // References point to a saved prefab. No runtime UI creation or local purchase grants.
    public sealed class AdRemovalOffer : MonoBehaviour
    {
        public RankGameApp app;
        public GameObject panel;
        public Button outside, close, buy, later, restore, lobbyOpen;
        public Text lobbyLabel;
        public Text title, subtitle, benefits, hintBenefit, permanent, buyLabel, laterLabel, restoreLabel, status;
        IAdRemovalPurchases purchases = new UnavailablePurchases();
        bool launchChecked, busy, destroyed, observedOwnership, manualOpening, displayedReady;
        string displayedPrice;
        int request;
        string statusKey="";
        GameObject previousFocus;
        public bool Visible => panel != null && panel.activeSelf;
        public bool OwnsAdRemoval => purchases.OwnsAdRemoval;
        bool Ready => app.AdsConnected && purchases.ProductReady && purchases.EntitlementChecked && !OwnsAdRemoval && !string.IsNullOrWhiteSpace(purchases.DisplayPrice);

        void Awake()
        {
            outside.onClick.AddListener(Dismiss);close.onClick.AddListener(Dismiss);later.onClick.AddListener(Dismiss);
            buy.onClick.AddListener(()=>Process(false));restore.onClick.AddListener(()=>Process(true));
            lobbyOpen.onClick.AddListener(OpenFromLobby);
        }
        public void SetPurchases(IAdRemovalPurchases service)
        {
            purchases=service??new UnavailablePurchases();request++;busy=false;statusKey="";
            CheckOwnership();if(Visible)RefreshText();
        }
        public void CheckLaunch(bool returningAtSafeLobby)
        {
            if(launchChecked)return;launchChecked=true;
            if(!returningAtSafeLobby || !Ready)return;
            Show(false);
        }
        public void OpenFromLobby()
        {
            if(!app.CanOpenAdRemoval || OwnsAdRemoval || Visible)return;
            launchChecked=true;Show(true);
        }
        void Show(bool manual)
        {
            manualOpening=manual;
            previousFocus=EventSystem.current==null?null:EventSystem.current.currentSelectedGameObject;
            RefreshText();panel.transform.SetAsLastSibling();panel.SetActive(true);
            if(EventSystem.current!=null)EventSystem.current.SetSelectedGameObject(close.gameObject);
        }
        public void Dismiss()
        {
            if(!Visible)return;panel.SetActive(false);
            if(EventSystem.current!=null)EventSystem.current.SetSelectedGameObject(previousFocus!=null && previousFocus.activeInHierarchy?previousFocus:null);
        }
        void Update()
        {
            CheckOwnership();
            if(!Visible)return;
            if(!app.CanOpenAdRemoval || (!manualOpening && !busy && !Ready)){Dismiss();return;}
            if(displayedReady!=Ready || displayedPrice!=purchases.DisplayPrice)RefreshText();
            // Keep keyboard navigation inside the saved modal, including Shift+Tab.
            if(Input.GetKeyDown(KeyCode.Tab) && EventSystem.current!=null)
            {
                var buttons=new[]{close,buy,later,restore};var selected=EventSystem.current.currentSelectedGameObject;
                int index=Array.FindIndex(buttons,b=>b.gameObject==selected),direction=Input.GetKey(KeyCode.LeftShift)||Input.GetKey(KeyCode.RightShift)?-1:1;
                for(int i=0;i<buttons.Length;i++){index=(index+direction+buttons.Length)%buttons.Length;if(buttons[index].IsInteractable()){EventSystem.current.SetSelectedGameObject(buttons[index].gameObject);break;}}
            }
        }
        void CheckOwnership()
        {
            if(OwnsAdRemoval && !observedOwnership){observedOwnership=true;busy=false;request++;Dismiss();app.AdRemovalVerified();}
            if(!OwnsAdRemoval)observedOwnership=false;
            if(lobbyOpen.gameObject.activeSelf==OwnsAdRemoval)
            {
                if(OwnsAdRemoval && EventSystem.current!=null && EventSystem.current.currentSelectedGameObject==lobbyOpen.gameObject)
                    EventSystem.current.SetSelectedGameObject(app.multiplayer.gameObject);
                lobbyOpen.gameObject.SetActive(!OwnsAdRemoval);
            }
        }
        void Process(bool restoring)
        {
            if(!Visible || busy || !Ready)return;
            busy=true;statusKey="ad_offer_pending";int token=++request;RefreshText();
            Action<PurchaseResult> complete=value=>
            {
                if(destroyed || token!=request)return;
                CheckOwnership();if(OwnsAdRemoval)return;
                // Completed is insufficient without the verified entitlement supplied by the adapter.
                busy=value==PurchaseResult.Pending || value==PurchaseResult.Completed;
                statusKey=busy?"ad_offer_pending":value==PurchaseResult.Cancelled?"":value==PurchaseResult.NothingToRestore?"ad_offer_none":"ad_offer_failed";
                if(!busy)request++;if(Visible)RefreshText();
            };
            try{if(restoring)purchases.Restore(complete);else purchases.Purchase(complete);}
            catch(Exception){complete(PurchaseResult.Failed);}
        }
        public void RefreshText()
        {
            displayedReady=Ready;displayedPrice=purchases.DisplayPrice;lobbyLabel.text=app.UiText("ad_offer_title");
            title.text=app.UiText("ad_offer_title");subtitle.text=app.UiText("ad_offer_subtitle");
            benefits.text=app.UiText("ad_offer_ads");hintBenefit.text=app.UiText("ad_offer_hint");permanent.text=app.UiText("ad_offer_permanent");
            buyLabel.text=Ready?app.UiFormat("ad_offer_buy",purchases.DisplayPrice):app.UiText("coming_soon");laterLabel.text=app.UiText("ad_offer_later");restoreLabel.text=app.UiText("ad_offer_restore");
            status.text=statusKey.Length==0?"":app.UiText(statusKey);buy.interactable=restore.interactable=!busy && Ready;
        }
        void OnDestroy(){destroyed=true;request++;}
    }
}
