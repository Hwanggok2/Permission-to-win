using System;
using System.Collections;
using UnityEngine;

namespace StageGame
{
    public sealed partial class RankGameApp
    {
        public AdRemovalOffer adOffer;
        public CanvasGroup lobbyInput;
        Coroutine lobbyAdRoutine;
        int navigationVersion;
        public bool AdsConnected => !(ads is NoFillAds);
        public bool AdRemovalOwned => adOffer!=null && adOffer.OwnsAdRemoval;
        public bool CanOpenAdRemoval => strings!=null && lobby.activeInHierarchy && !applicationPaused && !interstitialPending &&
            !help.activeSelf && !settings.activeSelf && !notices.activeSelf && !detail.gameObject.activeSelf && !online.panel.activeSelf;

        IEnumerator OfferAtLaunch(bool returning, int version)
        {
            // Let initial deep-link handling settle, then consume this launch's only opportunity.
            yield return null;yield return null;
            adOffer?.CheckLaunch(returning && navigationVersion==version && lobby.activeSelf && !applicationPaused &&
                !help.activeSelf && !settings.activeSelf && !detail.gameObject.activeSelf && !online.panel.activeSelf);
        }
        public bool RecordOnlineCompletion(string room,int round)
        {
            return progress!=null && progress.RecordOnlineResult(room,round);
        }
        public void ReturnFromMultiplayer(bool completedBeforeConfirmedLeave)
        {
            ShowLobby();
            if(completedBeforeConfirmedLeave && !applicationPaused && !AdRemovalOwned && AdsConnected)
                lobbyAdRoutine=StartCoroutine(LobbyAdBoundary());
        }
        IEnumerator LobbyAdBoundary()
        {
            bool resolved=false;int generation=adGeneration;
            interstitialPending=true;lobbyInput.interactable=false;RefreshMusic();
            try { ads.ShowInterstitial(value=>
            {
                if(destroyed || resolved || generation!=adGeneration || !lobby.activeSelf || online.panel.activeSelf || applicationPaused)return;
                resolved=true;
                if(value==AdResult.Completed){progress.Data.lastInterstitialUtc=DateTimeOffset.UtcNow.ToUnixTimeSeconds();progress.Save();}
            }); } catch(Exception) { resolved=true; }
            float deadline=Time.realtimeSinceStartup+4;
            while(!resolved && !applicationPaused && !AdRemovalOwned && Time.realtimeSinceStartup<deadline && generation==adGeneration)yield return null;
            resolved=true;
            if(generation==adGeneration){interstitialPending=false;ads.CancelPending();lobbyInput.interactable=true;RefreshMusic();}
            lobbyAdRoutine=null;
        }
        void CancelLobbyAd()
        {
            if(lobbyAdRoutine!=null)StopCoroutine(lobbyAdRoutine);lobbyAdRoutine=null;
            if(lobbyInput!=null)lobbyInput.interactable=true;
        }
        public void AdRemovalVerified()
        {
            ads.CancelPending();CancelPendingReward();
            if(interstitialPending){CancelResultSequence();if(result.activeSelf)SetResultActions(true);}
            RefreshBoard();RefreshMusic();
        }
    }
}
