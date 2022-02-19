namespace MVC.Model
{
    public interface IMoudle
    {
        void Create();
        void Release();
        void Update(float fDeltaTime);
        void LateUpdate();
        void OnApplicationPause(bool paused);
    }
}
