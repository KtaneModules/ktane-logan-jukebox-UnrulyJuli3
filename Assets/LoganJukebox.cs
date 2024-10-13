using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LoganJukebox : MonoBehaviour
{
    public KMSelectable[] Buttons;
    public TextMesh[] LyricTexts;
    public Material CorrectRecord;
    public KMBombModule Module;
    public KMAudio Audio;

    private class DumbLoganParodySong
    {
        public string AudioFile;
        public List<string> PossibleLyrics;
        public DumbLoganParodySong(string name, string lyrics)
        {
            AudioFile = name;
            PossibleLyrics = lyrics.Split(' ').ToList();
        }
    }

    /* private static string FormatLyric(string lyric)
    {
        string letterLyric = new string(lyric.Where(char.IsLetter).ToArray());
        return letterLyric.Substring(0, 1).ToUpper() + letterLyric.Substring(1).ToLower();
    } */

    private readonly static List<DumbLoganParodySong> dumbLoganParodySongs = new List<DumbLoganParodySong>
    {
        new DumbLoganParodySong("Bang", "Band Antique Weak Indie Billboard Wheel Yard Gone"),
        new DumbLoganParodySong("Blinding_Lights", "Internet Takis Driving Tests Theater"),
        new DumbLoganParodySong("Dance_Monkey", "Sing Dog Bark Earth Shocked Sparks George"),
        new DumbLoganParodySong("Finesse", "Rock Decades Music's Irrelevant Billboard Sense"),
        new DumbLoganParodySong("Good_as_Hell", "2009 Cable Memorize Periods Table"),
        new DumbLoganParodySong("Dont_Stop_The_Party", "Miracle Horrible Ignorable Unresistable Despicable Visible Digital Unoriginal Divisible Satirical Forgettable"),
        new DumbLoganParodySong("Kings_and_Queens", "Bad Catchy Hit Wonder Knows Year Gone Nobody"),
        new DumbLoganParodySong("Happy", "Bad Popular Despicable Song Seconds Singing Ears"),
        new DumbLoganParodySong("Friday", "Why Make Seconds Singing Weekend Stop Friends Vid Kill Torturing Shut Help"),
        new DumbLoganParodySong("Feel_It_Still", "Guy Girl Not Mind Peter Rabbit Music Exists Postman Revamped"),
        new DumbLoganParodySong("Nothing_On_You", "Anyone Songs Price Airplanes Hit Wonder Thrice Verse Nonsense Ironic Earth Flat"),
        new DumbLoganParodySong("Dear_Future_Husband", "Popular 2015 Songs Knows Saxophone Every Because Cool"),
        new DumbLoganParodySong("Starboy", "Song Bad Help Weeknd Forced Chorus Milk Views"),
        new DumbLoganParodySong("Ready_For_It", "Conspiracies False Boyfriend Mid-Dating Ridiculous Satan Shut"),
        new DumbLoganParodySong("My_Humps", "Appealing Feeling Roots Debut Jean TV Shit Spanish"),
        new DumbLoganParodySong("7_Rings", "Song Bragging Riches Specifics Lazy Effort Rap Nick Music Horrible Brag Wrong Everything"),
        //new DumbLoganParodySong("Dont", "Don't Yes Means Doctor Dangerous Cancer Sense"),
        new DumbLoganParodySong("Overwhelmed", "Song Eilish Rip-off Everyone Sound Crappy Dumb Dead Sea"),
        new DumbLoganParodySong("Havana", "Song Nah Sodium Tag Friends Instead Because Knows"),
        new DumbLoganParodySong("Bad_Guy", "Billie Eyelash Shift Keyboard Broken"),
        new DumbLoganParodySong("Blurred_Lines", "Now Thinking Budget Using Hashtags Video")
    };

    private static int mIDCounter;
    private int mID;

    private DumbLoganParodySong selectedSong;
    private List<int> correctOrder;

    void Start()
    {
        mID = ++mIDCounter;
        //Debug.LogFormat("[The Logan Parody Jukebox #{0}] ", mID);

        selectedSong = dumbLoganParodySongs.PickRandom();
        Debug.LogFormat("[The Logan Parody Jukebox #{0}] Selected song ID: {1}", mID, selectedSong.AudioFile);

        List<int> indexes = Enumerable.Range(0, selectedSong.PossibleLyrics.Count).ToList().Shuffle();
        List<int> selectedIndexes = new List<int>();
        for (int i = 0; i < LyricTexts.Length; i++)
        {
            LyricTexts[i].text = selectedSong.PossibleLyrics[indexes[i]];
            selectedIndexes.Add(indexes[i]);
        }

        correctOrder = Enumerable.Range(0, LyricTexts.Length).ToList();
        correctOrder.Sort((x, y) => selectedIndexes[x] - selectedIndexes[y]);

        Debug.LogFormat("[The Logan Parody Jukebox #{0}] Selected lyrics: {1}", mID, string.Join(", ", selectedIndexes.Select(i => selectedSong.PossibleLyrics[i]).ToArray()));
        Debug.LogFormat("[The Logan Parody Jukebox #{0}] Correct button order: {1}", mID, string.Join(", ", correctOrder.Select(i => (i + 1).ToString()).ToArray()));
        
        for (int i = 0; i < Buttons.Length; i++) AssignButton(i);
    }

    private int numPressed;
    private List<int> pressedButtons = new List<int>();

    void AssignButton(int i)
    {
        KMSelectable button = Buttons[i];
        button.OnInteract += delegate
        {
            button.AddInteractionPunch(0.7f);
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, button.transform);
            if (!pressedButtons.Contains(i))
            {
                string[] numberNames = new string[3] { "first", "second", "third" };
                Debug.LogFormat("[The Logan Parody Jukebox #{0}] Pressed {1} button, expected {2} button", mID, numberNames[i], numberNames[correctOrder[numPressed]]);
                if (i == correctOrder[numPressed])
                {
                    pressedButtons.Add(i);
                    button.GetComponent<Renderer>().material = CorrectRecord;
                    numPressed++;
                    if (numPressed >= 3) StartCoroutine(Solved());
                }
                else
                {
                    Debug.LogFormat("[The Logan Parody Jukebox #{0}] Striking!", mID);
                    Module.HandleStrike();
                }
            }
            return false;
        };
    }

    IEnumerator Solved()
    {
        yield return null;
        Debug.LogFormat("[The Logan Parody Jukebox #{0}] Module is solved! Playing audio: {1}", mID, selectedSong.AudioFile);
        Module.HandlePass();
        Audio.PlaySoundAtTransform("recordScratch1", transform);
        yield return new WaitForSeconds(0.778f);
        Audio.PlaySoundAtTransform(selectedSong.AudioFile, transform);
        yield break;
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = "!{0} press 132 (1 = top to 3 = bottom)";
#pragma warning restore 414

    KMSelectable[] ProcessTwitchCommand(string command)
    {
        string[] split = command.ToLowerInvariant().Split(' ');
        if (split.Length > 1 && split[0] == "press")
        {
            IEnumerable<int> split2 = split[1].ToCharArray().Select(c =>
            {
                int i;
                if (int.TryParse(c.ToString(), out i)) return i;
                return 0;
            });
            List<KMSelectable> buttons = new List<KMSelectable>();
            foreach (int i in split2) if (i > 0 && i < 4) buttons.Add(Buttons[i - 1]);
            return buttons.ToArray();
        }
        return null;
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        yield return null;
        while (numPressed < 3)
        {
            Buttons[correctOrder[numPressed]].OnInteract();
            yield return new WaitForSeconds(0.1f);
        }
    }
}
