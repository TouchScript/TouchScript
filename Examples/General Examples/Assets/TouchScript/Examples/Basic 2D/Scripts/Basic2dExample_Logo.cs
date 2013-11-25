using TouchScript.Gestures;
using UnityEngine;

public class Basic2dExample_Logo : MonoBehaviour {

	void Start ()
	{
	    GetComponent<PressGesture>().StateChanged += (sender, args) =>
	    {
	        switch (args.State)
	        {
	            case Gesture.GestureState.Recognized:
	                for (var i = 0; i < 3; i++)
	                {
	                    var angle = Quaternion.Euler(0, 0, 360/3*i);
	                    var rb = createCopy(transform.localPosition + angle*Vector3.right*transform.localScale.x*1.2f);
	                    rb.mass = GetComponent<Rigidbody2D>().mass/3;
                        rb.AddForce(angle * Vector2.right * 5000);
	                }
                    Destroy(gameObject);
	                break;
	        }
	    };
	}

    private Rigidbody2D createCopy(Vector3 position)
    {
        var obj = Instantiate(gameObject) as GameObject;
        obj.name = "Logo";
        var t = obj.transform;
        t.parent = transform.parent;
        t.position = position;
        t.eulerAngles = new Vector3(0, 0, Random.Range(0, 360));
        t.localScale = Vector3.one*transform.localScale.x*.6f;
        return obj.GetComponent<Rigidbody2D>();
    }
	

}
