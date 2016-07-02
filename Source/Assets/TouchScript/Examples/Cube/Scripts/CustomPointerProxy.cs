using TouchScript.Pointers;

namespace TouchScript.Examples.Cube 
{
    public class CustomPointerProxy : TouchScript.Behaviors.Visualizer.PointerProxy 
    {
        protected override void updateOnce(Pointer pointer) {
            if (pointer.InputSource is RedirectInput) Hide();
            
            base.updateOnce(pointer);
        }
    }
}