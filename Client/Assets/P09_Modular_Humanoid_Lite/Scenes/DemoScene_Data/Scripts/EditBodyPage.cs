using P09.Modular.Humanoid.Data;
using UnityEngine;
using UnityEngine.Events;

namespace P09.Modular.Humanoid
{
    public sealed class EditBodyPage : EditPage
    {
        [SerializeField] private OrientationObject _sexSwitcher;
        [SerializeField] private OrientationObject _hairStyleSwitcher;
        [SerializeField] private OrientationObject _hairColorSwitcher;
        [SerializeField] private OrientationObject _skinColorSwitcher;
        [SerializeField] private OrientationObject _eyeColorSwitcher;
        [SerializeField] private OrientationObject _facialHairSwitcher;
        [SerializeField] private OrientationObject _bustSizeSwitcher;

        [SerializeField] private Camera closeUpCamera;

        private bool IsPortrait => Screen.height > Screen.width;

        public override void Init()
        {
            base.Init(); // EditPage 기본 초기화

            InitSwitcher(_sexSwitcher);
            InitSwitcher(_hairStyleSwitcher);
            InitSwitcher(_hairColorSwitcher);
            InitSwitcher(_skinColorSwitcher);
            InitSwitcher(_eyeColorSwitcher);
            InitSwitcher(_facialHairSwitcher);
            InitSwitcher(_bustSizeSwitcher);
        }

        private void InitSwitcher(OrientationObject orientation, UnityAction<EditPartType, int> callback = null)
        {
            foreach (var go in new[] { orientation.portraitUIObj, orientation.landScapeUIObj })
            {
                if (go != null && go.TryGetComponent<HorizontalSwitcher>(out var switcher))
                {
                    switcher.Init(callback ?? _onChangePart);
                }
            }
        }

        public override void UpdateView(bool isReset = false)
        {
            OrientationObject[] switchers =
            {
                _sexSwitcher, _hairStyleSwitcher, _hairColorSwitcher,
                _skinColorSwitcher, _eyeColorSwitcher,
                _facialHairSwitcher, _bustSizeSwitcher,
            };

            foreach (var orientation in switchers)
            {
                foreach (var go in new[] { orientation.portraitUIObj, orientation.landScapeUIObj })
                {
                    if (go != null && go.TryGetComponent<HorizontalSwitcher>(out var switcher))
                    {
                        switcher.UpdateView();
                        if (isReset)
                            switcher.Reset();
                    }
                }
            }

            var sexId = CharacterCreateController.AvatarEditData.SexId;
            _facialHairSwitcher.SetUIActive(sexId == CharacterCreateController.MaleSexId);
            _bustSizeSwitcher.SetUIActive(sexId == CharacterCreateController.FemaleSexId);
            
            if (closeUpCamera != null)
            {
                var camPos = closeUpCamera.transform.position;
                float newX = CharacterCreateController.AvatarEditData.SexId == CharacterCreateController.MaleSexId ? -6f : -5.98f;

                closeUpCamera.transform.position = new Vector3(newX, camPos.y, camPos.z);
            }
        }
    }
}