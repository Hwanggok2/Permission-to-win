using System;
using System.Collections;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace StageGame
{
    public sealed partial class OnlineGameController
    {
        public Text lobbyRatingLabel;
        const string RatingKey="stagegame.multiplayer.rating.v1";
        const string RatingOwnerKey=RatingKey+".owner";
        string profileIdentity;
        int? lobbyRating;
        int profileVersion;
        bool profileInitialized,profileLoading;
        float profileQueriedAt=-60;
        public void RefreshLobbyRating()
        {
            string identity=PlayerPrefs.GetString(TokenKey,"");
            if(!profileInitialized || profileIdentity!=identity)
            {
                profileInitialized=true;profileIdentity=identity;profileVersion++;lobbyRating=null;
                if(identity.Length==0)lobbyRating=200;
                else if(PlayerPrefs.HasKey(RatingKey) && PlayerPrefs.GetString(RatingOwnerKey,"")==ProfileOwner(identity))lobbyRating=PlayerPrefs.GetInt(RatingKey);
            }
            if(lobbyRatingLabel!=null)lobbyRatingLabel.text=F("ai_rating",lobbyRating.HasValue?lobbyRating.Value.ToString():"—");
        }
        void ApplyLobbyProfile(OnlineState profile)
        {
            if(profile.rating<0)return;
            RefreshLobbyRating();lobbyRating=profile.rating;RefreshLobbyRating();
            // Only persist an authenticated server profile, never an invitation's opponent rating.
            if(!string.IsNullOrEmpty(profile.playerId) && profileIdentity.Length>0)
            {
                string owner=ProfileOwner(profileIdentity);
                if(PlayerPrefs.GetInt(RatingKey,-1)!=profile.rating || PlayerPrefs.GetString(RatingOwnerKey,"")!=owner)
                {PlayerPrefs.SetInt(RatingKey,profile.rating);PlayerPrefs.SetString(RatingOwnerKey,owner);PlayerPrefs.Save();}
            }
        }
        static string ProfileOwner(string identity)
        {using(var sha=SHA256.Create())return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(identity)));}
        public IEnumerator RefreshLobbyProfile()
        {
            if(profileLoading || opening || string.IsNullOrEmpty(endpoint))yield break;
            if(!Uri.TryCreate(endpoint,UriKind.Absolute,out var address))yield break;
            var uri=new UriBuilder(address){Scheme=address.Scheme=="wss"?"https":"http",Path="/v1/profile",Query=""};
            profileLoading=true;profileQueriedAt=Time.unscaledTime;
            RefreshLobbyRating();
            int version=profileVersion;string identity=PlayerPrefs.GetString(TokenKey,"");
            using(var request=UnityWebRequest.Get(uri.Uri.AbsoluteUri))
            {
                request.timeout=8;
                if(identity.Length>0)request.SetRequestHeader("Authorization","Bearer "+identity);
                var operation=request.SendWebRequest();
                while(!operation.isDone)yield return null;
                if(version==profileVersion && identity==PlayerPrefs.GetString(TokenKey,""))
                {
                    if(request.responseCode==401)
                    {lobbyRating=null;PlayerPrefs.DeleteKey(RatingKey);PlayerPrefs.DeleteKey(RatingOwnerKey);PlayerPrefs.Save();RefreshLobbyRating();}
                    else if(request.result==UnityWebRequest.Result.Success)
                    {
                        OnlineState profile=null;try{profile=JsonUtility.FromJson<OnlineState>(request.downloadHandler.text);}catch(ArgumentException){}
                        if(profile?.type=="profile")ApplyLobbyProfile(profile);
                    }
                }
            }
            profileLoading=false;
        }
        void OnApplicationFocus(bool focused)
        {
#if !UNITY_EDITOR
            if(focused && profileInitialized && !opening && Time.unscaledTime-profileQueriedAt>=60)StartCoroutine(RefreshLobbyProfile());
#endif
        }
    }
}
