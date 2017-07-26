/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Behaviors.Cursors;
using TouchScript.Pointers;

namespace TouchScript.Examples.Cube 
{
    /// <exclude />
    public class CustomPointerProxy : PointerCursor
    {
        protected override void updateOnce(IPointer pointer) {
            if (pointer.InputSource is RedirectInput) Hide();
            
            base.updateOnce(pointer);
        }
    }
}