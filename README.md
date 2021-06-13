# Easybots Apps - Компилација од неколку ботови за Easybots

## • РЕЗИМЕ
Овој проект претставува компилација од неколкуте ботови/апликации за Easybots.

[Easybots](https://easybots.net/Docs), нашиот стартап (моментално во stand-by фаза), претставува алатка за автоматизација на задачи и креирање на персонални софтверски
решенија за неколку минути, наместо часови или денови.
Развиена е во периодот од 2017-2019, уште пред да бидат во тренд no-code алатките, и направена е со цел да им овозможи на девелоперите
и ентузијастите да ја [автоматизираат својата работа](https://www.it.mk/polesno-avtomatizirane-na-zdodevnite-protsesi-so-noviot-dansko-makedonski-startap-easybot/?fbclid=IwAR1ZGx9TvwdblkaOJ7MrksJLsmL-GypQ7JMrt5c1Tb9aZAIP1A8670IXXKU).

Автоматизација на задачите се врши преку креирање на персонализирани софтверски решенија, таканаречени Easybots Solutions.
Овие Easybots Solutions, се креираат со drag & drop принцип на визуелно програмирање, а нивните составни компоненти се "Тригери" (налик на евенти во C#),
и "Акции" (налик на методи во C#).

Во овој проект, референцирани се дел од Easybots ботовите, за кои може повеќе да погледнете на нашата веб страница, во секцијата [Public Apps](https://easybots.net/AppStore).
Обработени се ботовите за:
- uTorrent Client (овозможува манипулирање со uTorrent)
- YouTube App (овозможува пуштање, паузирање, пребарување, рандомизирање на YouTube песни и плејлисти)
- Exceptionless App (овозможува интеграција со [Exceptionless.com](https://exceptionless.com/) real-time error информаторот)
- IP Camera App (овозможува конекција и манипулација со IP Camera)

• Интересен податок
Во 2018 година, [држев презентација на ФИНКИ](https://www.facebook.com/events/211750806380018/?ref=newsfeed), преку секцијата Microsoft Students Club, во која го презентирав Easybots и зборував за дел од овие ботови.


## • Објаснување и опишување на класа од изворниот код
Како пример, ќе ја опишам класата "uTorrentClientBot" од uTorrentApp проектот.
uTorrentClientBot претставува Easybot, кој има функција да служи како "man-in-the-middle" помеѓу Easybots Studio (да прима команди) и 
UTorrentClient (составен дел од NuGet package-от, C# API за uTorrent).

За да се разбере суштината, ќе одам со TopDown approach каде ќе ги објаснам главните концепти на класата.
Пред се', мора да се има на ум првото правило од Easybots SDK-то:

**Секоја КЛАСА може да се претвори во Easybots, а секој МЕТОД во ТРИГЕР или АКЦИЈА**


Откако ќе го инсталираме Easybots SDK-то преку NuGet, правиме класата uTorrentClientBot да наследува од Easybots.Apps.Easybots.

```class uTorrentClientBot : Easybot```

Во base конструкторот треба да се специфицира името на ботот, и истото ќе биде прикажано во Easybots Studio.

```public uTorrentClientBot (string ip, int port, string username, string password) : base("uTorrentClient Bot", false)```


Методите кои сакаме да бидат акции, мора да бидат public, и им ставаме декоратор **[Easybots.Apps.Action]**.

Овие акции можат да бидат повикувани од Easybots Studio, и претставуваат операциите кои се извршуваат во секој Easybots Solution.
``` cs
[Action("Starts downloading torrent from your available torrents in your uTorrent Client, by a given torrent name. " +
            "If the torrent doesn't exist, InvalidOperationException will be thrown.")]
        public void StartDownloadingByTorrentName(
            [ParameterDescription("torrentName", "The name of the torrent.", typeof(string), AllowUserInput = true)]
            string torrentName)
        {
            if (string.IsNullOrWhiteSpace(torrentName))
                throw new ArgumentException("Insert valid torrent name. Your input: '" + torrentName + "'");

            lock (this.syncLockCommands)
            {
                TorrentBot torrentBot = this.torrentBots.FirstOrDefault(t => t.Torrent.Name == torrentName);
                if(torrentBot == null)
                    throw new InvalidOperationException(string.Format("Torrent with name '{0}', doesn't exist.", torrentName));

                torrentBot.StartDownloading();
            }
        }
```
Овој метод, е специфициран и деклариран како Easybots Action, и неговата цел е да започне со download-ирање на одреден торент, специфициран по неговото име.
Како опис на акцијата, во параметар се специфицира што точно ќе прави оваа акција, со цел истото да биде прикажано во Easybots Studio.
Дополнително, аргументите во методот StartDownloadingByTorrentName се опишани со Easybots деклараторот [ParameterDescription], кој служи да се опише каков ќе биде очекуваниот влез на информацијата од страната на Easybots Studio.

Exceptions кои се фрлаат во овој метод, директно се појавуваат и печатат во Easybots Studio конзолата (ова е регулирано од самиот engine на Easybots).

Функцијата на методот е да го најде торентот чие име е специфицирано, и да го повика методот StartDownloading().

Интересен детал е делот со `lock (this.syncLockCommands)`. Ова е направено на овој начин, со цел да се handle-а конкурентноста.
Од Easybots Studio може да се стартуваат повеќе Solutions, и доколку се јави конкурентност, може да настанат грешки. За таа цел,
методот е заклучен со lock(instance variable).

Методите пак, кои сакаме да бидат EasybotsTriggers, ги декларираме со **[Easybots.Apps.Trigger]** декораторот.
Тригерите се круцијални, бидејќи тие се почетокот на Easybots Solution-от.
Доколку немаме дефинирано тригер, Easybots Solution-от нема да може да започне.
Тригерите го нотифицираат Easybots Studio кога ќе се случи некој event во апликацијата.
``` cs
[Trigger("Notifies when the torrent is downloaded and returns its name.")]
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        [return: ParameterDescription("torrent name", "The name of the torrent.", typeof(string), AllowUserInput = false)]
        public string OnDownloadFinished(string torrentName)
        {
            this.TriggerInEasybotsPlatform(torrentName);
            return torrentName;
        }
```
Исто како кај акциите, се специфицира декоратор со аргумент опис на тригерот, и преку методот TriggerInEasybotsPlatform (кој доаѓа од класата Easybot, која горе ја наследивме),
се повикува тригер во Easybots Studio, кој како резултат во враќа името на торентот.
Овој тригер има цел да нотифицира кога одреден торент завршил со симнување.

## • ПРИКАЗ НА ГРАДЕЊЕТО НА EASYBOTS SOLUTION-ОТ ЗА UTORRENTBOT
![Torrent Web Remote Control Solution-от](https://i.imgur.com/HgW62Rh.png)

Името на Solution-от е "Torrent Web remote Control".
Во игра се 3 тригера, чија функција е:
- На прикачување на фајл (во Easybots Studio) да започнат со превземање на торентот (повикување на акцијата DownloadTorrent)
- На селектирање на item од Easybots List (ова е друг бот, кој е комбиниран во овој solution, и целта му е да рендерира податоци во List View. Име имплементирани тригери и акции, исто како и uTorrent ботот), да го сетира името на торентот
- На селектирање на "команда" (исто Easybots List ботот), да провери дали селектираната команда се совпаѓа со некое предефинирано име (ова е вградена функционалност во Easybots Studio која манифестира IF/ELSE гранења)
- И да повикува акции според селектираната команда, како на пример: Start/StopDownloadingByTorrentName, GetProgressByTorrentName (враќа progress на торентот како стринг), итн.

Овој solution може да го превземете од нашата страница, во делот за [Public Solutions Store](https://easybots.net/Solutions) и да го run-нувате во Easybots Studio.

## • ВИДЕО ПРИМЕРИ И УПАТСТВА ЗА КОРИСТЕЊЕ НА EASYBOTS И EASYBOTS БОТОВИТЕ
- Прати email со нотификација кога одреден фајл ќе се промени
https://youtu.be/mkpA8B93D-I
- Туторијал за како да креирате Easybots Solutions
https://youtu.be/9Aey6ASpteY
- Креирање solution за добивање карта за претставата "Вардарски пастуви" (видео наратирано и демонстрирано од моја страна)
https://youtu.be/FdD4VeFWaC8

## NOTE: Доколку сакате да го пробате Easybots Studio, слободно превземете го на www.easybots.net, и слободно пишете ми за 'Standard' или 'Pro' subscription.
## Дополнително, возможно е дел од овие ботови да не работат поради промени во API-јата кои ги користиме, бидејќи продуктот повеќе не се одржува поради фокус на наш друг стартап - [Howitzer](https://howitzer.co/)
