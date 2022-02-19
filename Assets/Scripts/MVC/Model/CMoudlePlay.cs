using MVC.Model.UiLogic;

namespace MVC.Model
{
    public class CMoudlePlay : IMoudle
    {

        #region UiLogic

        private UiStartWndLogic _uiStartWndLogic;

        public UiStartWndLogic UiStartWndLogic
        {
            get
            {
                if (_uiStartWndLogic==null)
                {
                    _uiStartWndLogic=new UiStartWndLogic();
                }
                return _uiStartWndLogic;
            }
        } 

        #endregion
        
        public void Create()
        {
           
        }

        public void Release()
        {
            if (_uiStartWndLogic!=null)
            {
                _uiStartWndLogic.Close();
                _uiStartWndLogic = null;
            }
        }

        public void Update(float fDeltaTime)
        {
            
        }

        public void LateUpdate()
        {
            
        }

        public void OnApplicationPause(bool paused)
        {
           
        }
    }
}
