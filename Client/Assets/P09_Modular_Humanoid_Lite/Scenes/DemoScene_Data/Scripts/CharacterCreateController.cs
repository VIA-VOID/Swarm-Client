using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using P09.Modular.Humanoid.Data;
using UnityEngine.Events;
using UnityEngine.UI;

namespace P09.Modular.Humanoid
{
    public class CharacterCreateController : SceneController
    {
        [Header("View")]
        [SerializeField] private List<EditPage> _editPages;
        [SerializeField] private List<AvatarView> _avatarViews;
        [SerializeField] private OrientationObject _transitionView;
        [SerializeField] private OrientationObject _resetButton;

        [Header("EditPartData")]
        [SerializeField] private List<EditPartDataContainer> _editPartDataList;

        private static PageType _currentPage = PageType.Body;
        private static List<EditPartDataContainer> EditPartDataList;
        public static AvatarEditData AvatarEditData;

        public static readonly int MaleSexId = 1;
        public static readonly int FemaleSexId = 2;

        private static readonly HashSet<EditPartType> BodyPageHidePartTypes = new()
        {
            EditPartType.Head,
            EditPartType.Weapon,
            EditPartType.Shield
        };

        public enum PageType
        {
            None = -1,
            Body,
            Armor,
            Weapon,
        }

        private void Start() => Init();

        private void Init()
        {
            _currentPage = PageType.Body;
            AvatarEditData = new AvatarEditData();
            EditPartDataList = _editPartDataList;

            foreach (var editPage in _editPages)
            {
                if (editPage == null) continue;
                
                editPage.SetEventHandlers(OnChangePage, OnChangePart, OnRotateModel, OnChangeFaceEmotion);
                editPage.Init();
            }

            foreach (var avatarView in _avatarViews)
                avatarView.Init();
            
            if (_editPages.Count <= (int)_currentPage || _editPages[(int)_currentPage] == null)
            {
                Debug.LogError($"[CharacterCreateController] PageType({_currentPage})에 해당하는 EditPage가 없습니다.");
                return;
            }

            _editPages[(int)_currentPage].Show();
            //OnChangePage(_currentPage);
            
            UpdateView();
        }

        public void ChangePage()
        {
            var nextPage = _currentPage == PageType.Body ? PageType.Armor : PageType.Body;
            OnChangePage(nextPage);
        }

        public void OnClickCreateCharacter()
        {
            PlayerData newPlayer = new PlayerData();

            newPlayer.appearance = new CharacterAppearanceData();
            
            // 현재 선택된 외형 정보를 기반으로 PlayerData 생성
            newPlayer.appearance.sexId        = AvatarEditData.SexId;
            newPlayer.appearance.hairStyleId  = AvatarEditData.HairStyleId;
            newPlayer.appearance.hairColorId  = AvatarEditData.HairColorId;
            newPlayer.appearance.skinColorId  = AvatarEditData.SkinId;
            newPlayer.appearance.eyeColorId   = AvatarEditData.EyeColorId;
            newPlayer.appearance.facialHairId = AvatarEditData.FacialHairId;
            newPlayer.appearance.bustSizeId   = AvatarEditData.BustSizeId;
            
            //선택한 직업에 따라 무기 설정
            
            // 기본 장비 착용

            // 데이터 저장
            DataManager.Instance.SetUserData(newPlayer);

            // 씬 이동 (3번 씬)
            UnityEngine.SceneManagement.SceneManager.LoadScene(3);
        }
        
        private void InitOrientationButton(OrientationObject buttonObj, UnityAction action)
        {
            foreach (var go in new[] { buttonObj.portraitUIObj, buttonObj.landScapeUIObj })
            {
                if (go.TryGetComponent<Button>(out var btn))
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(action);
                }
            }
        }

        private void InitOrientationComponent<T>(OrientationObject orientationObj, Action<T> initializer) where T : Component
        {
            foreach (var go in new[] { orientationObj.portraitUIObj, orientationObj.landScapeUIObj })
            {
                if (go.TryGetComponent<T>(out var comp))
                {
                    initializer.Invoke(comp);
                }
            }
        }

        private void UpdateView(bool isReset = false)
        {
            foreach (var avatarView in _avatarViews)
                avatarView?.UpdateView();

            foreach (var editPage in _editPages)
            {
                if (editPage == null) continue;
                editPage.UpdateView(isReset);
            }
        }

        private void OnChangePage(PageType pageType)
        {
            if (_currentPage == pageType)
                return;

            _currentPage = pageType;

            for (int i = 0; i < _editPages.Count; i++)
            {
                if (i == (int)pageType)
                    _editPages[i].Show();
                else
                    _editPages[i].Hide();
            }

            //InitOrientationComponent(_transitionView, (TransitionView t) => t.ChangeTransition(pageType == PageType.Body));
            UpdateView();
        }

        private void OnChangePart(EditPartType partType, int contentId)
        {
            Debug.Log($"Change part: {partType}");
            var currentId = AvatarEditData.GetCurrentId(partType);
            AvatarEditData.SetId(partType, currentId == contentId ? 0 : contentId);
            UpdateView();
        }

        private void OnChangeFaceEmotion(int emotionId)
        {
            Debug.Log($"Change face emotion: {emotionId}");
            var dataList = EditPartDataList
                .FirstOrDefault(d => d.Type == EditPartType.FaceEmotion)
                ?.PartDataList;

            if (dataList != null)
            {
                var currentData = dataList.FirstOrDefault(d => d.ContentId == emotionId) as FaceEmotionData;
                _avatarViews[(int)PageType.Body].ChangeFaceEmotion(currentData?.AnimationName);
            }

            UpdateView();
        }

        private void OnRotateModel(bool isLeft, bool isDown)
        {
            Debug.Log($"Rotate model: {isLeft}, {isDown}");
            _avatarViews[(int)_currentPage].RotateModel(isLeft, isDown);
        }

        public static (int currentId, List<IEditPartData> dataList) GetEditPartData(EditPartType type)
        {
            var selectId = AvatarEditData.GetCurrentId(type);
            if (_currentPage == PageType.Body && BodyPageHidePartTypes.Contains(type))
                selectId = 0;

            var dataList = EditPartDataList
                .FirstOrDefault(d => d.Type == type && d.SexId == 0)?.PartDataList;

            if (dataList is { Count: > 0 })
                return (selectId, dataList);

            dataList = EditPartDataList
                .FirstOrDefault(d => d.Type == type && d.SexId == AvatarEditData.SexId)?.PartDataList;

            return (selectId, dataList);
        }

        public static List<IEditPartData> GetAnyEditPartDataList(EditPartType type)
        {
            return EditPartDataList.FirstOrDefault(d => d.Type == type)?.PartDataList;
        }

        private void OnValidate()
        {
            var editPartTypes = Enum.GetValues(typeof(EditPartType)).Cast<EditPartType>().ToList();
            var duplicateTypes = editPartTypes
                .GroupBy(x => x)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateTypes.Count > 0)
            {
                Debug.LogError($"Duplicate EditPartType: {string.Join(", ", duplicateTypes)}");
            }
        }
    }
}
