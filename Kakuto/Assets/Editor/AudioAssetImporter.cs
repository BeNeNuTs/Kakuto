using UnityEditor;

public class AudioAssetPostProcessor : AssetPostprocessor
{
    void OnPreprocessAudio()
    {
        AudioImporter audioImporter = (AudioImporter)assetImporter;
        audioImporter.forceToMono = true;
        AudioImporterSampleSettings settings = audioImporter.defaultSampleSettings;
        bool isMusic = assetPath.Contains("Music");
        settings.loadType = isMusic ? UnityEngine.AudioClipLoadType.Streaming : UnityEngine.AudioClipLoadType.DecompressOnLoad;
        settings.compressionFormat = isMusic ? UnityEngine.AudioCompressionFormat.PCM : UnityEngine.AudioCompressionFormat.Vorbis;
    }
}