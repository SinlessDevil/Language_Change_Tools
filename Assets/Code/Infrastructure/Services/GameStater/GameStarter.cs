using Code.Infrastructure.Factory;
using Code.Infrastructure.Services.PersistenceProgress;
using Code.Infrastructure.Services.PersistenceProgress.Player;
using Code.Infrastructure.Services.SaveLoad;
using Code.Localization.Code.Services.LocalizeLanguageService;
using UnityEngine;

namespace Code.Infrastructure.Services.GameStater
{
    public class GameStarter : IGameStarter
    {
        private readonly IPersistenceProgressService _progressService;
        private readonly ISaveLoadService _saveLoadService;
        private readonly IUIFactory _uiFactory;
        private readonly ILocalizeLanguageService _localizeLanguageService;

        public GameStarter(
            IPersistenceProgressService progressService,
            ISaveLoadService saveLoadService, 
            IUIFactory uiFactory,
            ILocalizeLanguageService localizeLanguageService)
        {
            _progressService = progressService;
            _saveLoadService = saveLoadService;
            _uiFactory = uiFactory;
            _localizeLanguageService = localizeLanguageService;
        }

        public void Initialize()
        {
            Debug.Log("GameStarter.Initialize");
            
            InitProgress();
            InitUI();
            InitLanguage();
        }
        
        private void InitProgress()
        {
            _progressService.PlayerData = LoadProgress() ?? SetUpBaseProgress();   
        }
        
        private void InitUI()
        {
            _uiFactory.CreateUIRoot();
            _uiFactory.CreateGameHud();
        }
        
        private void InitLanguage()
        {
            _localizeLanguageService.InitLanguage();
        }
        
        private PlayerData LoadProgress()
        {
            return _saveLoadService.Load();
        }

        private PlayerData SetUpBaseProgress()
        {
            var progress = new PlayerData();
            _progressService.PlayerData = progress;
            return progress;
        }
    }
}