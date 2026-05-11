# Snow Surfer Mini Game

Bu proje tek sahneli prototipten mobil odakli 2D endless runner yapisina cevrildi.

## Oynanis

- Play modda oyun `Tap to Start` ekraniyla acilir.
- Mobilde veya editor mouse testinde basili tutup surukleyerek snowboarder karakteri yonlendirilir.
- Editorde `WASD` veya yon tuslari da calisir.
- Engellerden kac, gift collectible objelerini topla.
- Skor hayatta kalma suresiyle artar; gift toplamak ekstra skor ve combo verir.
- Engele cok yakin gecmek near-miss bonusu verir.
- Oyun ilerledikce hiz artar ve spawn araliklari kisalir.
- Carpinca game over acilir; final skor, best skor, toplanan gift, near-miss ve sure gorunur.
- Game over ekraninda tiklayarak veya `Restart` butonuyla hizli yeniden baslatilir.
- `P` veya `Esc` pause; HUD uzerindeki pause butonu da ayni isi yapar.

## Sistemler

- `GameManager`: Start, Playing, Paused, GameOver akisi.
- `PlayerController`: touch/mouse drag ve klavye kontrolu, ekran sinirlari, tilt ve crash davranisi.
- `SpawnManager`: adil lane secimiyle obstacle ve collectible spawn.
- `ObjectPool`: obstacle/collectible icin pooling.
- `ScoreManager`: skor, high score, combo, near-miss, PlayerPrefs kaydi.
- `DifficultyManager`: sureye bagli hiz ve spawn zorlugu.
- `MissionManager`: run ici mini hedefler ve bonuslar.
- `AudioManager`: dis ses dosyasi olmadan DSP ile ding, thud, start, game-over ve combo sesleri.
- `ParallaxBackground`: snow tile, ridge ve cloud hareketi.
- `UIManager`: start, HUD, pause ve game-over panelleri.

## Sprite Kullanimi

- `Snowboarding Dino`: oyuncu karakteri.
- `Floor Tile Fill`: kayan kar zemini.
- `Floor Tile`: pist/ridge dekoru.
- `Cloud 1`, `Cloud 2`, `Cloud 3`: parallax bulutlar.
- `Rock 1`, `Rock 2`: erken oyun engelleri.
- `Post 1`, `Post 2`: orta oyun engelleri.
- `Tree 1`, `Tree 2`, `Tree 3`, `Tree 4`, `Tree Fallen`: zorluk arttikca gelen engeller.
- `Gift Bag`: collectible/odul.
- `Snowboarder Frog`: alternatif karakter adayi olarak uygun, fakat ilk surumde oyuncu okunabilirligini bozmamak icin sahneye alinmadi.

## Editor Notu

Sahne/prefab kurulumunu tekrar uretmek icin Unity menusu:

`Snow Surfer > Rebuild Playable Scene`

Bu islem `Assets/Prefabs`, `Assets/Scripts` ve `Assets/Scenes/SampleScene.unity` uzerinden sahneyi tekrar kurar.
