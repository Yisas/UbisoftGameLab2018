using UnityEngine;
using System.Collections;

//basic utility class to destroy objects on startup
public class DestroyObject : MonoBehaviour 
{
	public AudioClip destroySound;	//sound to play when object is destroyed
	public float delay;				//delay before object is destroyed
	public bool destroyChildren;	//should the children be detached (and kept alive) before object is destroyed?
	public float pushChildAmount;	//push children away from centre of parent
	
	
	void Start()
	{
		//get list of children
		Transform[] children = new Transform[transform.childCount];
		for (int i = 0; i < transform.childCount; i++)
			children[i] = transform.GetChild(i);
		
		//detach children
		if (!destroyChildren)
			transform.DetachChildren();
		
		//add force to children (and a bit of spin)
		foreach (Transform child in children)
		{
			if(child.GetComponent<Rigidbody>() && pushChildAmount != 0)
			{
				Vector3 pushDir = child.position - transform.position;
				child.GetComponent<Rigidbody>().AddForce(pushDir * pushChildAmount, ForceMode.Force);
				child.GetComponent<Rigidbody>().AddTorque(Random.insideUnitSphere, ForceMode.Force);
			}
		}
		
		//destroy  parent
		if(destroySound)
			AudioSource.PlayClipAtPoint(destroySound, transform.position);
		Destroy (gameObject, delay);
	}
}