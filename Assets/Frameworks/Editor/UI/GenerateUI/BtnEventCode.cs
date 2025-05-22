public class BtnEventCode
{
    public static string BtnEvtInitFormat =
        @"
        UIEventListener.OnClick(#名字#Btn.gameObject).AddListener(OnClick#名字#Btn);";
    
    public static string BtnEvtUnInitFormat =
        @"
        UIEventListener.OnClick(#名字#Btn.gameObject).RemoveListener(OnClick#名字#Btn);";

    public static string BtnFuncFormat =
        @"
        public void OnClick#名字#Btn(GameObject obj)
        {
            
        }
        
        ";
}