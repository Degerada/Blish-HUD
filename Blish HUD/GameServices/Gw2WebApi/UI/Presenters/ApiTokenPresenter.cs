﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Gw2WebApi.UI.Views;
using Gw2Sharp.WebApi.V2;
using Gw2Sharp.WebApi.V2.Models;

namespace Blish_HUD.Gw2WebApi.UI.Presenters {
    public class ApiTokenPresenter : Presenter<ApiTokenView, string> {

        private static readonly Logger Logger = Logger.GetLogger<ApiTokenPresenter>();

        private readonly CancellationTokenSource _loadCancel;

        private TokenInfo                _tokenInfo;
        private Account                  _accountInfo;
        private IApiV2ObjectList<string> _characters;

        public ApiTokenPresenter(ApiTokenView view, string apiKey) : base(view, apiKey) {
            _loadCancel = new CancellationTokenSource();
        }

        protected override async Task<bool> Load(IProgress<string> progress) {
            var tokenClient = GameService.Gw2WebApi.GetConnection(this.Model).Client.V2;

            try {
                var tokenInfoTask     = tokenClient.TokenInfo.GetAsync(_loadCancel.Token);
                var accountInfoTask   = tokenClient.Account.GetAsync(_loadCancel.Token);
                var characterInfoTask = tokenClient.Characters.IdsAsync(_loadCancel.Token);

                var loadTasks = new Dictionary<Task, string>() {
                    {tokenInfoTask, "Token Info"},
                    {accountInfoTask, "Account Details"},
                    {characterInfoTask, "Characters"}
                };

                while (loadTasks.Count > 0) {
                    progress.Report($"Loading {string.Join(", ", loadTasks.Values)}...");
                    var completed = await Task.WhenAny(loadTasks.Keys);
                    loadTasks.Remove(completed);
                }

                progress.Report("Handling response from API...");

                return UpdateFromRequestTaskResult(tokenInfoTask,     ref _tokenInfo)
                    && UpdateFromRequestTaskResult(accountInfoTask,   ref _accountInfo)
                    && UpdateFromRequestTaskResult(characterInfoTask, ref _characters);
            } catch (Exception ex) {
                HandleErrorLoading(ex);
            }

            return true;
        }

        private bool UpdateFromRequestTaskResult<T>(Task<T> infoTask, ref T field) {
            if (infoTask.IsCanceled) return false;

            if (infoTask.Exception != null) {
                Logger.Warn(infoTask.Exception, "Request failed.");
                HandleErrorLoading(infoTask.Exception);
                return false;
            }

            field = infoTask.Result;
            return true;
        }

        private void HandleErrorLoading(Exception ex) {
            _loadCancel.Cancel();
        }

        protected override void UpdateView() {
            this.View.TokenInfo     = _tokenInfo;
            this.View.AccountInfo   = _accountInfo;
            this.View.CharacterList = _characters;

            this.View.Active = GameService.Gw2WebApi.PrivilegedConnection.Connection.AccessToken == this.Model;

            base.UpdateView();
        }

        protected override void Unload() {
            _loadCancel.Cancel();

            base.Unload();
        }

    }
}
