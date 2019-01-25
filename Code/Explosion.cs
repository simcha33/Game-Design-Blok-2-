using UnityEngine;
using System.Collections;

public class Explosion : MonoBehaviour
{
    public float minimumDamage = 5.0f;
    public float maximumDamage = 50.0f;

    public float minimumKnockbackForce = 5.0f;
    public float maximumKnockbackForce = 50.0f;

    private void Start()
    {
        StartCoroutine(DisableHitbox());
    }

    private IEnumerator DisableHitbox()
    {
        yield return new WaitForSeconds(1.0f);

        GetComponent<SphereCollider>().enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        float dx = other.transform.position.x - transform.position.x;
        float dy = other.transform.position.y - transform.position.y;
        float d = Mathf.Sqrt(dx * dx + dy * dy);
        float a = Mathf.Atan2(dy, dx);

        float p = d / GetComponent<SphereCollider>().radius;
        other.GetComponent<Player>().Damage((int)Mathf.Lerp(minimumDamage, maximumDamage, p));

        other.GetComponent<Rigidbody>().AddForce(new Vector3(Mathf.Cos(a), Mathf.Sin(a), .0f) * Mathf.Lerp(minimumKnockbackForce, maximumKnockbackForce, p), ForceMode.Impulse);
    }
}
