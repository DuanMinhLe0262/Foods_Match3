using UnityEngine;

public class SfxManager : MonoBehaviour
{
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip swapClip;
    [SerializeField] private AudioClip invalidSwapClip;
    [SerializeField] private AudioClip clearClip;
    [SerializeField] private AudioClip boardRegenClip;
    [SerializeField] private AudioClip outOfMovesClip;

    private void Play(AudioClip clip)
    {
        if (sfxSource == null || clip == null) return;
        sfxSource.PlayOneShot(clip);
    }

    public void PlaySwap() => Play(swapClip);
    public void PlayInvalidSwap() => Play(invalidSwapClip);
    public void PlayClear() => Play(clearClip);
    public void PlayBoardRegen() => Play(boardRegenClip);
    public void PlayOutOfMoves() => Play(outOfMovesClip);
}