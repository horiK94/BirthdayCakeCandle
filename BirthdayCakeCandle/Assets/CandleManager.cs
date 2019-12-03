using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CandleManager : MonoBehaviour
{
    /// <summary>
    /// サンプリング数
    /// </summary>
    private readonly int SampleNum = (2 << 9);
    /// <summary>
    /// 音量
    /// </summary>
    [SerializeField, Range(0f, 1000f)]
    private float volume = 200f;
    /// <summary>
    /// 録音再生用AudioSource
    /// </summary>
    private AudioSource audioSource = null;
    /// <summary>
    /// サンプリングして取得した値
    /// </summary>
    private float[] currentValues;

    /// <summary>
    /// キャンドルプレファブ
    /// </summary>
    [SerializeField]
    private GameObject[] candlePrefab = null;
    /// <summary>
    /// キャンドルの数
    /// </summary>
    [SerializeField, Range(1, 10)]
    private int candleNum = 1;
    /// <summary>
    /// キャンドル消滅最大音量
    /// </summary>
    [SerializeField]
    private float maxVolume = 0.001f;
    /// <summary>
    /// キャンドル消滅最小音量
    /// </summary>
    [SerializeField]
    private float minVolume = 0.0000001f;

    CandleRenderer[] candleRenderer = null;

    // Use this for initialization
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        currentValues = new float[SampleNum];
        if (audioSource == null)
        {
            Debug.LogAssertion("audioSourceをアタッチしてください");
            return;
        }
        if (Microphone.devices.Length == 0)
        {
            //マイクが見つからないとき
            Debug.LogAssertion("マイクを接続してください");
            return;
        }
        string devName = Microphone.devices[0]; // 複数見つかってもとりあえず0番目のマイクを使用
        int minFreq, maxFreq = 0;
        Microphone.GetDeviceCaps(devName, out minFreq, out maxFreq); // 最大最小サンプリング数を得る
        int ms = minFreq / SampleNum; // サンプリング時間を適切に取る
        audioSource.loop = true; // ループにする
        audioSource.clip = Microphone.Start(devName, true, ms, minFreq); // clipをマイクに設定
        while (!(Microphone.GetPosition(devName) > 0)) { } // きちんと値をとるために待つ
        Microphone.GetPosition(null);
        audioSource.Play();

        candleRenderer = new CandleRenderer[candleNum];
        Vector3 leftPos = new Vector3(-candleNum + 1, 0, 0);
        for (int i = 0; i < candleNum; i++)
        {
            int r = Random.Range(0, candlePrefab.Length);
            GameObject obj = Instantiate(candlePrefab[r]) as GameObject;
            obj.transform.position = leftPos + new Vector3(2 * i, 0, 0);
            candleRenderer[i] = obj.GetComponent<CandleRenderer>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        audioSource.GetSpectrumData(currentValues, 0, FFTWindow.Hamming);
        float sum = 0f;
        for (int i = 0; i < currentValues.Length; ++i)
        {
            sum += currentValues[i]; // データ（周波数帯ごとのパワー）を足す
        }
        // データ数で割ったものに倍率をかけて音量とする
        float volumeRate = Mathf.Clamp01(sum * volume / (float)currentValues.Length);
        Debug.Log(volumeRate);

        //キャンドルの火を消す
        if (volumeRate < minVolume)
        {
            return;
        }

        float t = (volumeRate - minVolume) / (maxVolume - minVolume);
        for (int i = 0; i < candleNum; i++)
        {
            float judge = Random.Range(0.0f, 1.0f);
            if (t > judge)
            {
                candleRenderer[i].Disappear();
            }
        }
    }
}
