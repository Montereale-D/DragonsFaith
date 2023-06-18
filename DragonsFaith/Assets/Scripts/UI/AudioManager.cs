using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace UI
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager instance { get; private set; }
        
        [SerializeField] private SoundBank soundBank;

        [SerializeField] private AudioSource backgroundMusic;
        [SerializeField] private AudioSource uiSound;
        [SerializeField] private AudioSource enemySound;
        [SerializeField] private AudioSource playerSound;

        protected void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                instance = this;
                DontDestroyOnLoad(this);
            }
        }

        #region UI
        
        public void PlayOverUIButtonSound()
        {
            uiSound.PlayOneShot(soundBank.OverUIButton);
        }
        public void PlayClickUIButtonSound()
        {
            uiSound.PlayOneShot(soundBank.ClickUIButton);
        }
        
        #endregion

        #region PlayerSFX
        
        public void PlayPlayerWalkSound()
        {
            playerSound.PlayOneShot(soundBank.PlayerWalk);
            playerSound.loop = true;
        }
        
        public void StopPlayerWalkSound()
        {
            //playerSound.Stop();
            playerSound.loop = false;
        }
        
        public void PlayEnemyWalkSound()
        {
            enemySound.PlayOneShot(soundBank.PlayerWalk);
        }
        
        public void StopEnemyWalkSound()
        {
            //enemySound.PlayOneShot(soundBank.PlayerWalk);
            enemySound.loop = false;
        }
        public void PlayPLayerRewardSound()
        {
            playerSound.PlayOneShot(soundBank.PlayerReward);
        }
        public void PlayPLayerDashSound()
        {
            playerSound.PlayOneShot(soundBank.PlayerDash);
        }
        public void PlayPLayerHurtSound()
        {
            playerSound.PlayOneShot(soundBank.PlayerHurt);
            //playerSound.PlayOneShot(soundBank.PlayerHurt[Random.Range(0, soundBank.PlayerHurt.Count)]);
        }
        public void PlayPLayerDieSound()
        {
            playerSound.PlayOneShot(soundBank.PlayerDie);
        }
        
        #endregion

        #region Knife

        public void PlayPlayerKnifeAttackSound()
        {
            playerSound.PlayOneShot(soundBank.knifeAttack);
        }
        
        public void PlayEnemyKnifeAttackSound()
        {
            enemySound.PlayOneShot(soundBank.knifeAttack);
        }

        #endregion
        
        #region Pistol

        public void PlayPlayerPistolAttackSound()
        {
            playerSound.PlayOneShot(soundBank.pistolAttack);
        }
        
        public void PlayPlayerPistolReloadSound()
        {
            playerSound.PlayOneShot(soundBank.pistolReload);
        }

        #endregion
        
        #region AssaultRifle

        public void PlayPlayerAssaultAttackSound()
        {
            playerSound.PlayOneShot(soundBank.assaultRifleAttack);
        }
        
        public void PlayPlayerAssaultReloadSound()
        {
            playerSound.PlayOneShot(soundBank.assaultRifleReload);
        }
        
        public void PlayEnemyAssaultAttackSound()
        {
            enemySound.PlayOneShot(soundBank.assaultRifleAttack);
        }
        
        public void PlayEnemyAssaultReloadSound()
        {
            enemySound.PlayOneShot(soundBank.assaultRifleReload);
        }

        #endregion
        
        #region Shotgun

        public void PlayShotgunAttackSound()
        {
            playerSound.PlayOneShot(soundBank.shotgunAttack);
        }
        
        public void PlayShotgunReloadSound()
        {
            playerSound.PlayOneShot(soundBank.shotgunReload);
        }

        #endregion
        
        #region SniperRifle

        public void PlaySniperAttackSound()
        {
            playerSound.PlayOneShot(soundBank.sniperRifleAttack);
        }
        
        public void PlaySniperReloadSound()
        {
            playerSound.PlayOneShot(soundBank.sniperRifleReload);
        }

        #endregion

        #region Music
        
        public void PlaySoundTrackMenu()
        {
            backgroundMusic.clip = soundBank.SoundTrackMenu;
            backgroundMusic.Play();
            Debug.Log("playing");
        }

        public void StopSoundTrack()
        {
            backgroundMusic.Stop();
        }
        
        public void PlaySoundTrackExplore()
        {
            backgroundMusic.Stop();
            backgroundMusic.clip = soundBank.SoundTrackExplore;
            backgroundMusic.Play();
        }

        public void StopSoundTrackExplore()
        {
            backgroundMusic.Stop();
            backgroundMusic.clip = soundBank.SoundTrackMenu;
            backgroundMusic.Play();
        }
        
        public void PlaySoundTrackCombat()
        {
            backgroundMusic.Stop();
            backgroundMusic.clip = soundBank.SoundTrackCombat;
            backgroundMusic.Play();
        }

        public void StopSoundTrackCombat()
        {
            backgroundMusic.Stop();
            backgroundMusic.clip = soundBank.SoundTrackExplore;
            backgroundMusic.Play();
        }
        
        #endregion

        #region Environment

        public void PlayObstacleDestroyedSound()
        {
            playerSound.PlayOneShot(soundBank.obstacleDestruction);
        }
        
        public void PlayOpenChestSound()
        {
            playerSound.PlayOneShot(soundBank.openingChest);
        }
        
        public void PlayOpenGateSound()
        {
            playerSound.PlayOneShot(soundBank.openingGate);
        }
        
        public void PlayOpenHiddenSound()
        {
            playerSound.PlayOneShot(soundBank.openingHiddenArea);
        }
        
        public void PlayUseItemSound()
        {
            playerSound.PlayOneShot(soundBank.usingItem);
        }
        
        public void PlaySpottedSound()
        {
            playerSound.PlayOneShot(soundBank.spotted);
        }

        #endregion
    }
}
