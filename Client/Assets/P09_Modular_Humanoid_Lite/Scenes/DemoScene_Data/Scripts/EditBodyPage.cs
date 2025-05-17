using P09.Modular.Humanoid.Data;
using UnityEngine;
using UnityEngine.Events;

namespace P09.Modular.Humanoid
{
    public sealed class EditBodyPage : EditPage
    {
        [SerializeField] private OrientationObject  _sexSwitcher;
        [SerializeField] private OrientationObject  _hairStyleSwitcher;
        [SerializeField] private OrientationObject  _hairColorSwitcher;
        [SerializeField] private OrientationObject  _skinColorSwitcher;
        [SerializeField] private OrientationObject  _eyeColorSwitcher;
        [SerializeField] private OrientationObject  _facialHairSwitcher;
        [SerializeField] private OrientationObject  _bustSizeSwitcher;
        
        [SerializeField] private OrientationObject  _faceEmotionSwitcher;
        
        private bool IsPortrait => Screen.height > Screen.width;

        private void InitSwitcher(OrientationObject orientation)
        {
            var switcher = orientation.GetUIObj(IsPortrait).GetComponent<HorizontalSwitcher>();
            switcher.Init(_onChangePart);
        }

        public override void UpdateView(bool isReset = false)
        {
            OrientationObject[] switchers = {
                _sexSwitcher, _hairStyleSwitcher, _hairColorSwitcher,
                _skinColorSwitcher, _eyeColorSwitcher,
                _facialHairSwitcher, _bustSizeSwitcher,
                _faceEmotionSwitcher
            };

            foreach (var orientation in switchers)
            {
                var switcher = orientation.GetUIObj(IsPortrait).GetComponent<HorizontalSwitcher>();
                switcher.UpdateView();
                if (isReset)
                    switcher.Reset();
            }

            var sexId = DemoPageController.AvatarEditData.SexId;
            _facialHairSwitcher.SetUIActive(sexId == DemoPageController.MaleSexId);
            _bustSizeSwitcher.SetUIActive(sexId == DemoPageController.FemaleSexId);
        }
    }
}