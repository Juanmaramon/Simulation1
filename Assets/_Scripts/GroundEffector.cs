using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundEffector : MonoBehaviour {
    const int NUMBER_SAMPLES = 256;
    const int NUMBER_BANDS = 7;
    float[] spectrumLeft = new float[NUMBER_SAMPLES];
    float[] spectrumRight = new float[NUMBER_SAMPLES];
    float[] audioBandBuffer = new float[NUMBER_BANDS];

    float[] freqBand = new float[NUMBER_BANDS];
    float[] bandBuffer = new float[NUMBER_BANDS];
    float[] bufferDecrease = new float[NUMBER_BANDS];
    float[] freqBandHightest = new float[NUMBER_BANDS];
    public float colorIncrement;
    float amplitudeHightest;
    float amplitudeBuffer;
    public float maxFrequencyScale;

    MaterialPropertyBlock _props;
    [SerializeField] MeshRenderer _rend;
    [SerializeField]
    private AudioSource audioSource;

    void Start()
    {
        _props = new MaterialPropertyBlock();
    }
	
	void Update () 
    {
        audioSource.GetSpectrumData(spectrumLeft, 0, FFTWindow.Blackman);
        audioSource.GetSpectrumData(spectrumRight, 1, FFTWindow.Blackman);

        // Amplitude computation
        MakeFrequencyBands();
        CreateAudioBands();
        GetAmplitude();

        _props.SetFloat("_Value1", amplitudeBuffer);
        _rend.SetPropertyBlock(_props);
	}

    void MakeFrequencyBands()
    {
        int count = 0;
        float average = 0f;
        int sampleCount;

        for (int i = 0; i < NUMBER_BANDS; i++)
        {

            average = 0f;
            sampleCount = (int)Mathf.Pow(2, i) * 2;

            if (i == NUMBER_BANDS - 1)
            {
                sampleCount += 2;
            }

            for (int j = 0; j < sampleCount; j++)
            {
                average += spectrumLeft[count] + spectrumRight[count] * (count + 1);
                count++;
            }

            average /= count;

            freqBand[i] = average * maxFrequencyScale;

            BandBuffer(i);
        }
    }

    void BandBuffer(int i)
    {
        if (freqBand[i] > bandBuffer[i])
        {
            bandBuffer[i] = freqBand[i];
            bufferDecrease[i] = 0.0005f;
        }
        if (freqBand[i] < bandBuffer[i])
        {
            bandBuffer[i] -= bufferDecrease[i];
            bufferDecrease[i] *= 1.2f;
        }
    }

    void CreateAudioBands()
    {
        for (int i = 0; i < NUMBER_BANDS; i++)
        {
            if (freqBand[i] > freqBandHightest[i])
            {
                freqBandHightest[i] = freqBand[i];
            }
            audioBandBuffer[i] = (bandBuffer[i] / freqBandHightest[i]) * colorIncrement;
        }
    }

    void GetAmplitude()
    {
        float currentAmplitudeBuffer = 0f;

        for (int i = 0; i < NUMBER_BANDS; i++)
        {
            currentAmplitudeBuffer += audioBandBuffer[i];
        }

        if (currentAmplitudeBuffer > amplitudeHightest)
        {
            amplitudeHightest = currentAmplitudeBuffer;
        }

        amplitudeBuffer = currentAmplitudeBuffer / amplitudeHightest;
    }
}
