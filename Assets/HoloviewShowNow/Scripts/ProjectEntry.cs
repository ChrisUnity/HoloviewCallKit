
using UnityEngine;
using MyFramework;

namespace ShowNow
{
    public class ProjectEntry : ScriptSingleton<ProjectEntry>
    {


        private void Update()
        {
            UpdateLocated(); 
        }
        public void UploadWorldAnchor()
        {

        }
        #region 锚点更新逻辑
        public bool NeedLocated = false;
        public void UpdateLocated()
        {
            if (NeedLocated)
            {
                RaycastHit hitinfo;
#if UNITY_IOS
                if (Physics.Raycast(ResourceManager.Instance.IpadCamera.transform.position, ResourceManager.Instance.IpadCamera.transform.forward, out hitinfo, 10, 1 << 31))
                #else
                if (Physics.Raycast(ResourceManager.Instance.HololensCamera.transform.position, ResourceManager.Instance.HololensCamera.transform.forward, out hitinfo, 10, 1 << 31))
#endif
                {
                    ResourceManager.Instance.AnchorObj.transform.position = hitinfo.point;
                }
                else
                {
                    ResourceManager.Instance.AnchorObj.transform.position= Camera.main.transform.position + 3 * Camera.main.transform.forward;
                }
            }
        }
        public void StartLocate()
        {
            NeedLocated = true;
        }
        public void EndLocate()
        {
            NeedLocated = false;

        }
        #endregion




    }



}

