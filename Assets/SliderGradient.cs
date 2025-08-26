using UnityEngine;
using UnityEngine.UI;

public class SliderGradient : MonoBehaviour
{
    public Gradient fillGradient;
    public Image fillImage;
    public Slider slider;

    private void OnEnable()
    {
        slider.onValueChanged.AddListener(OnSliderValueChanged);
    }
    private void OnDisable()
    {
        slider.onValueChanged.RemoveListener(OnSliderValueChanged);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        slider = GetComponent<Slider>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnSliderValueChanged(float value)
    {
        //Set the fill image color to be the gradient's value based on our fill.
        fillImage.color = fillGradient.Evaluate(value);
    }
}
