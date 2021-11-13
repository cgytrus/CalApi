using System.Collections;
using System.Reflection;

using CalApi.API.Cat;

using Cat;

using UnityEngine;

namespace CalApi.Core;

public class LightBulb : MonoBehaviour {
    /*private bool _triggered;
    private FadeEffect _effect;
    private FieldInfo _speed;
    
    private void OnTriggerEnter2D(Collider2D other) {
        if(_triggered || !other.GetComponent<CatPart>() || Camera.main == null)
            return;
        _triggered = true;

        Transform camera = Camera.main.transform;
        GameObject effectObj = Instantiate(GameObject.Find("Fade To White Effect"), camera.position,
            Quaternion.identity, camera);
        _effect = effectObj.GetComponent<FadeEffect>();
        _speed = AccessTools.Field(_effect.GetType(), "speed");
        StartCoroutine(nameof(FadeCoroutine));
    }

    private IEnumerator FadeCoroutine() {
        _speed.SetValue(_effect, float.PositiveInfinity);
        _effect.FadeTo(Color.white);
        yield return new WaitForSeconds(3f);
        foreach(SpriteRenderer renderer in GetComponentsInChildren<SpriteRenderer>()) renderer.enabled = false;
        CalApiPlugin.controls.ApplyColor(Color.white);
        _speed.SetValue(_effect, 0.5f);
        _effect.FadeTo(Color.clear);
    }*/
}