using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FullScreenDamageController : MonoBehaviour {
    [SerializeField] private ScriptableRendererFeature damageEffect;
    [SerializeField] private Material damageEffectMaterial;
    [SerializeField] [Range(0.2f, 2f)] private float displayTime;
    [SerializeField] [Range(0.2f, 2f)] private float fadeOutTime;
    
    // private
    private int _vignetteIntensityId = Shader.PropertyToID("_VignetteIntensity");
    private float _vignetteIntensity = 1.7f;

    private void Start() {
        damageEffect.SetActive(false);
    }

    public void TakeDamage() {
        StartCoroutine(TakeDamageInternal());
    }

    private IEnumerator TakeDamageInternal() {
        damageEffect.SetActive(true);
        damageEffectMaterial.SetFloat(_vignetteIntensityId, _vignetteIntensity);
        yield return new WaitForSeconds(displayTime);

        float elapsedTime = 0f;
        while (elapsedTime < fadeOutTime) {
            elapsedTime += Time.deltaTime;
            float newIntensity = Mathf.Lerp(_vignetteIntensity, 0f, (elapsedTime / fadeOutTime));
            damageEffectMaterial.SetFloat(_vignetteIntensityId, newIntensity);

            yield return null;
        }
        
        damageEffect.SetActive(false);
    }
}