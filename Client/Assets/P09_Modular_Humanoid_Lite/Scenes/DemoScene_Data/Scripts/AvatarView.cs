using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using P09.Modular.Humanoid.Data;

namespace P09.Modular.Humanoid
{
    public class AvatarView : MonoBehaviour
    {
        [SerializeField] private Transform _modelRoot;
        [SerializeField] private Animator _animator;
        [SerializeField] private RuntimeAnimatorController _maleAnimatorController;
        [SerializeField] private RuntimeAnimatorController _femaleAnimatorController;
        
        private CharacterCreateController.PageType _pageType;
        private bool _isRotating = false;
        private bool _isLeft = false;
        private int _currentSexId;

        private const float RotateSpeed = 100f;
        private const string SkinMaterialPattern = @"^P09_.*_Skin.*$";
        private const string EyeMaterialPattern = @"^P09_Eye.*$";

        private static readonly EditPartType[] EquipmentTypes = new[]
        {
            EditPartType.Weapon,
            EditPartType.Shield,
            EditPartType.Head,
            EditPartType.Chest,
            EditPartType.Arm,
            EditPartType.Waist,
            EditPartType.Leg
        };

        public void Init()
        {
            _isRotating = false;
            _isLeft = false;
            _modelRoot.localEulerAngles = Vector3.zero;
        }
        
        private void Update()
        {
            if (!_isRotating)
            {
                return;
            }

            var rotateSpeed = RotateSpeed * Time.deltaTime;
            var rotateY = _isLeft ? rotateSpeed : -rotateSpeed;
            _modelRoot.Rotate(0, rotateY, 0);
        }

        public void ChangeFaceEmotion(string faceEmotionName)
        {
            _animator.Play(faceEmotionName);
        }
        
        public void RotateModel(bool isLeft, bool isRotate)
        {
            _isLeft = isLeft;
            _isRotating = isRotate;
        }

        public void UpdateView()
        {
            _currentSexId = CharacterCreateController.AvatarEditData.SexId;

            _animator.runtimeAnimatorController = _currentSexId == CharacterCreateController.MaleSexId
                ? _maleAnimatorController
                : _femaleAnimatorController;
            foreach (Transform child in GetComponentsInChildren<Transform>(true))
            {
                // Sex
                var (currentSexId, sexDataList) = CharacterCreateController.GetEditPartData(EditPartType.Sex);
                UpdateRenderer(child, currentSexId, sexDataList);
                // HairStyle
                var (currentHairStyleId, hairStyleDataList) = CharacterCreateController.GetEditPartData(EditPartType.HairStyle);
                UpdateRenderer(child, currentHairStyleId, hairStyleDataList);
                // HairColor
                var (currentHairColorId, hairColorDataList) = CharacterCreateController.GetEditPartData(EditPartType.HairColor);
                UpdateHairColor(child, currentHairColorId, hairColorDataList);
                // SkinColor
                var (currentSkinColorId, skinColorDataList) = CharacterCreateController.GetEditPartData(EditPartType.Skin);
                UpdateSkinColor(child, currentSkinColorId, skinColorDataList);
                // EyeColor
                var (currentEyeColorId, eyeColorDataList) = CharacterCreateController.GetEditPartData(EditPartType.EyeColor);
                UpdateEyeColor(child, currentEyeColorId, eyeColorDataList);
                if (_currentSexId == CharacterCreateController.MaleSexId)
                {
                    // FacialHair
                    var (currentFacialHairId, facialHairDataList) = CharacterCreateController.GetEditPartData(EditPartType.FacialHair);
                    UpdateRenderer(child, currentFacialHairId, facialHairDataList);
                }
                else if (_currentSexId == CharacterCreateController.FemaleSexId)
                {
                    // BustSize
                    var (currentBustSizeId, bustSizeDataList) = CharacterCreateController.GetEditPartData(EditPartType.BustSize);
                    UpdateBustSize(child, currentBustSizeId, bustSizeDataList);
                }
                
                // Equipment
                foreach (var equipmentType in EquipmentTypes)
                {
                    var (currentEquipmentId, equipmentDataList) = CharacterCreateController.GetEditPartData(equipmentType);
                    UpdateRenderer(child, currentEquipmentId, equipmentDataList);
                }
            }
        }
        
        private void UpdateRenderer(Transform child, int currentId, List<IEditPartData> dataList)
        {
            foreach (var data in dataList)
            {
                if (child.name == data.MeshName)
                {
                    child.gameObject.SetActive(data.ContentId == currentId);
                }
                else if(child.name == string.Format(data.MeshName, "Male"))
                {
                    child.gameObject.SetActive(_currentSexId == CharacterCreateController.MaleSexId && data.ContentId == currentId);
                }
                else if (child.name == string.Format(data.MeshName, "Female") || child.name == string.Format(data.MeshName, "Fem"))
                {
                    child.gameObject.SetActive(_currentSexId == CharacterCreateController.FemaleSexId && data.ContentId == currentId);
                }
            }
        }

        private void UpdateColor(Transform child, int currentId, List<IEditPartData> dataList)
        {
            var currentData = dataList.FirstOrDefault(d => d.ContentId == currentId); 
            foreach (var data in dataList)
            {
                if (child.name == string.Format(data.MeshName, data.DisplayName))
                {
                    var renderer = child.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.material = (currentData as ColorEditPartData)?.Material;
                    }
                }
            }
        }
        
        private void UpdateHairColor(Transform child, int currentId, List<IEditPartData> dataList)
        {
            var currentData = dataList.FirstOrDefault(d => d.ContentId == currentId); 
            var hairStyleId = CharacterCreateController.AvatarEditData.HairStyleId;
            foreach (var data in dataList)
            {
                if (child.name == string.Format(data.MeshName, hairStyleId))
                {
                    var renderer = child.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.material = (currentData as HairColorEditPartData)?.GetMaterial(hairStyleId);
                    }
                }
            }
        }
        
        private void UpdateSkinColor(Transform child, int currentId, List<IEditPartData> dataList)
        {
            var renderer = child.GetComponent<Renderer>();
            if (renderer == null) return;
            Material[] materials = renderer.materials;
            for (int i = 0; i < materials.Length; i++)
            {
                var material = materials[i];
                if (Regex.IsMatch(material.name, SkinMaterialPattern))
                {
                    var currentData = dataList.FirstOrDefault(d => d.ContentId == currentId);
                    materials[i] = (currentData as ColorEditPartData)?.Material;
                }
            }
            renderer.materials = materials;
        }

        private void UpdateEyeColor(Transform child, int currentId, List<IEditPartData> dataList)
        {
            var currentData = dataList.FirstOrDefault(d => d.ContentId == currentId);
            if (currentData != null && !child.name.Contains(currentData.MeshName)) return;
            var renderer = child.GetComponent<Renderer>();
            if (renderer == null) return;
            Material[] materials = renderer.materials;
            for (int i = 0; i < materials.Length; i++)
            {
                var material = materials[i];
                if (Regex.IsMatch(material.name, EyeMaterialPattern))
                {
                    materials[i] = (currentData as ColorEditPartData)?.Material;
                }
            }
            renderer.materials = materials;
        }

        private void UpdateBustSize(Transform child, int currentId, List<IEditPartData> dataList)
        {
            var currentData = dataList.FirstOrDefault(d => d.ContentId == currentId) as BustSizeEditPartData;
            if (currentData == null) return;
            if (child.name != string.Format(currentData.MeshName, "R") &&
                child.name != string.Format(currentData.MeshName, "L")) return;
            child.localScale = currentData.Size;
        }
    }
}
