/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Pointers;

namespace TouchScript.Examples.Cube 
{
    public class CustomPointerProxy : Behaviors.Visualizer.PointerProxy 
    {
        protected override void updateOnce(Pointer pointer) {
            if (pointer.InputSource is RedirectInput) Hide();
            
            base.updateOnce(pointer);
        }
    }
}