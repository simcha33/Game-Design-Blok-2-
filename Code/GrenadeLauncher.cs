using UnityEngine;
using System.Collections;

public class GrenadeLauncher : MonoBehaviour
{
    public int PlayerID { get; set; }

    public GameObject grenadePrefab;
    public Transform nozzle;

    public GameObject chargeBar;

    public Camera playerCamera;

    public float minimumShootForce = 50.0f;
    public float maximumShootForce = 250.0f;

    public float maxChargeTime = 3.0f;

    private float chargeTimer;
    private bool isCharging;

    void Start()
    {
        
    }
    
    void Update()
    {
        Vector2 screenPos = playerCamera.WorldToScreenPoint(transform.position);

        Vector2 targetPos;
        if (PlayerID == 1) targetPos = Input.mousePosition;
        else targetPos = screenPos + new Vector2(Input.GetAxis("ShootX"), -Input.GetAxis("ShootY"));

        float dx = targetPos.x - screenPos.x;
        float dy = targetPos.y - screenPos.y;
        float a = Mathf.Atan2(dy, dx);
        float angle = a * Mathf.Rad2Deg;

        transform.localEulerAngles = new Vector3(angle * Mathf.Sign(transform.parent.localScale.z) + (transform.parent.localScale.z > .0f ? 180 : 0), .0f, .0f);

        if(isCharging)
        {
            chargeTimer += Time.deltaTime;

            if(ShouldShoot())
            {
                float shootForce = Mathf.Lerp(minimumShootForce, maximumShootForce, Mathf.Min(chargeTimer, maxChargeTime) / maxChargeTime);

                var grenadeObject = Instantiate(grenadePrefab, nozzle.position, Quaternion.identity);
                grenadeObject.GetComponent<Rigidbody>().AddForce(new Vector3(Mathf.Cos(a), Mathf.Sin(a), .0f) * shootForce, ForceMode.Impulse);

                isCharging = false;
                if (chargeTimer > maxChargeTime) chargeTimer = maxChargeTime;
            }
        }
        else
        {
            chargeTimer -= Time.deltaTime;

            if (ShouldStartCharging())
            {
                isCharging = true;
                chargeTimer = .0f;
            }
        }

        chargeBar.transform.localScale = new Vector3(1.0f, Mathf.Clamp(chargeTimer / maxChargeTime, .0001f, 1.0f), 1.0f);
    }

    private bool ShouldStartCharging()
    {
        if (chargeTimer > .0f) return false;

        if(Input.GetAxis(string.Format("FireP{0}", PlayerID)) > .5f)
        {
            return true;
        }

        return false;
    }

    private bool ShouldShoot()
    {
        if (!isCharging) return false;

        if(Input.GetAxis(string.Format("FireP{0}", PlayerID)) < .1f)
        {
            return true;
        }

        return false;
    }
}
