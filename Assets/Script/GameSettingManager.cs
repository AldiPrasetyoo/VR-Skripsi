using UnityEngine;


public class GameSettingManager : MonoBehaviour
{

    public static GameSettingManager Instance;


    private const string QualityKey = "GameQuality";
    private const string VolumeKey = "GameVolume";



    private void Awake()
    {

        if(Instance == null)
        {

            Instance = this;

            DontDestroyOnLoad(gameObject);

        }
        else
        {

            Destroy(gameObject);

        }


        LoadSetting();

    }




    public void SetQuality(int index)
    {

        QualitySettings.SetQualityLevel(index);

        PlayerPrefs.SetInt(
            QualityKey,
            index
        );

        PlayerPrefs.Save();

    }





    public void SetVolume(float value)
    {

        AudioListener.volume = value;


        PlayerPrefs.SetFloat(
            VolumeKey,
            value
        );


        PlayerPrefs.Save();

    }





    void LoadSetting()
    {


        int quality =
        PlayerPrefs.GetInt(
            QualityKey,
            QualitySettings.GetQualityLevel()
        );



        float volume =
        PlayerPrefs.GetFloat(
            VolumeKey,
            1f
        );



        QualitySettings.SetQualityLevel(
            quality
        );


        AudioListener.volume =
        volume;


    }



}