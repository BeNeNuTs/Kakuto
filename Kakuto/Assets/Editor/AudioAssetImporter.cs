using UnityEditor;

public class AudioAssetPostProcessor : AssetPostprocessor
{
    void OnPreprocessAudio()
    {
        AudioImporter audioImporter = (AudioImporter)assetImporter;
        AudioImporterSampleSettings settings = audioImporter.defaultSampleSettings;
        bool isMusic = assetPath.Contains("Music");
        if(!isMusic)
        {
            audioImporter.forceToMono = true;
        }
        settings.loadType = isMusic ? UnityEngine.AudioClipLoadType.Streaming : UnityEngine.AudioClipLoadType.DecompressOnLoad;
        settings.compressionFormat = isMusic ? UnityEngine.AudioCompressionFormat.PCM : UnityEngine.AudioCompressionFormat.Vorbis;
        audioImporter.defaultSampleSettings = settings;
    }
}