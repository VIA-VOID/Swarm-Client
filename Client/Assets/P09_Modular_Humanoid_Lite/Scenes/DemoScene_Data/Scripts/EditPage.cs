using UnityEngine;
using UnityEngine.Events;
using P09.Modular.Humanoid.Data;

namespace P09.Modular.Humanoid
{
    public abstract class EditPage : MonoBehaviour
    {
        [SerializeField] private OrientationObject _root;
        [SerializeField] private OrientationObject _modelRotateButton;
        
        protected bool _isActive;
        protected UnityAction<EditPartType, int> _onChangePart;
        protected UnityAction<CharacterCreateController.PageType> _onChangePage;
        protected UnityAction<int> _onChangeFaceEmotion;

        private bool IsPortrait => UIManager.Instance.GetIsPortrait();
        
        public virtual void Init()
        {
            UIManager.OnOrientationChanged += HandleOrientationChanged;
            Hide();
        }
        
        protected virtual void OnDestroy()
        {
            UIManager.OnOrientationChanged -= HandleOrientationChanged;
        }
        
        private void HandleOrientationChanged(bool isPortrait)
        {
            if (_isActive)
                UpdateView(); // 방향 바뀔 때 UI 동기화
        }
        
        public void SetEventHandlers(
            UnityAction<CharacterCreateController.PageType> onChangePage,
            UnityAction<EditPartType, int> onChangePart,
            UnityAction<bool, bool> onRotateModel,
            UnityAction<int> onChangeFaceEmotion)
        {
            _onChangePage = onChangePage;
            _onChangePart = onChangePart;
            _onChangeFaceEmotion = onChangeFaceEmotion;

            foreach (var go in new[] { _modelRotateButton.portraitUIObj, _modelRotateButton.landScapeUIObj })
            {
                if (go != null && go.TryGetComponent<ModelRotateButton>(out var button))
                    button.Init(onRotateModel);
            }
        }

        public void Show()
        {
            _isActive = true;
            _root.SetUIActive(true);
        }
        
        public void Hide()
        {
            _isActive = false;
            _root.SetUIActive(false);
        }

        public abstract void UpdateView(bool isReset = false);
    }
}