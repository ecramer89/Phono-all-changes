
using UnityEngine;
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using Extensions;
public class Parameters : MonoBehaviour {

	[SerializeField] InputType inputType;

	void Start ()
	{     
		//ensure that problem sets are sorted according to session number.
		//expedites retrieval of session data from session index.
		StudentMode.PROBLEM_SETS.Sort((SessionData x, SessionData y) => x.number - y.number);
	
		Events.Dispatcher.RecordInputTypeSelected (inputType);
		Events.Dispatcher.OnActivitySelected += (Activity obj) => {
			Application.LoadLevel ("Activity");
		};


		SpellingRuleRegex.Test();
			
	}
		

	public class UI{
		public static readonly int ONSCREEN_LETTER_SPACES = 6;
	}

	public class FILEPATHS{
		public static readonly string RESOURCES_WORD_IMAGE_PATH = "WordImages/"; 

	}

	public class Colors{

		//define special colors to use over Unity's blue, green, yellow, red, etc.
		static Color green = new Color (
			(float)95 / (float)255,
			(float)180 / (float)255,
			(float)127 / (float)255
		);

		static Color blue = new Color (
			(float)105 / (float)255,
			(float)210 / (float)255,
			(float)231 / (float)255
        );

		static Color red = new Color (
			1f,
			(float)58 / (float)255,
			(float)68 / (float)255
      	);

		static Color yellow = new Color (
			(float)249 / (float)255,
			(float)249 / (float)255,
			(float)98 / (float)255
		);

		public static Color DEFAULT_ON_COLOR=Color.white;
		public static Color DEFAULT_OFF_COLOR=Color.gray;

		public class OpenClosedVowelColors{
			public static Color SHORT_VOWEL = yellow;
			public static Color LONG_VOWEL = red;
			public static Color FIRST_CONSONANT_COLOR = blue;
			public static Color SECOND_CONSONANT_COLOR = green;
		}


		public class MagicEColors{
			public static Color INNER_VOWEL = red;
			public static Color SILENT_E = Color.gray;
		}

		public class ConsonantDigraphColors{
			public static Color COMPLETED_DIGRAPH_COLOR = green;
			public static Color SINGLE_MEMBER_OF_TARGET_DIGRAPH_COLOR = Color.gray;

		}

		public class ConsonantBlendColors{
			public static Color COMPLETED_BLEND_COLOR = green;
			public static Color SINGLE_CONSONANT_COLOR = blue;

		}

		public class VowelDigraphColors{
			public static Color COMPLETED_DIGRAPH_COLOR = red;
			public static Color SINGLE_MEMBER_OF_TARGET_DIGRAPH_COLOR = Color.gray;

		}

		public class RControlledVowelColors{
			public static Color R_CONTROLLED_VOWEL_COLOR = red;//applies to both the vowel and the r.
			//separate colors for the r and the vowel will require minor changes to the code in the r controlled vowel
			//colorer.
			public static Color SINGLE_MEMBER_OF_TARGET_R_CONTROLLED_VOWEL_COLOR = Color.gray;
		}

	}

	public class StudentMode{
		public static int PROBLEMS_PER_SESSION;

		/*
		 * template for defining a session:
		 * new SessionData(
		 *   sessionNumber (begin counting at 0, so session 1-first session = 0),
		 *   activity type (e.g. OPEN_CLOSED_SYLLABLE. please refer to Activity enum in enums file)
		 *   an array of target words
		 *   an array of initial "placeholder" letters.
		 *   REQUIREMENTS:
		 *     -the lengths of target and placeholder words must be equal
		 *     -each session needs to have the same number of problems (system assumes this for now)
		 *     the constructor for session data will throw exception if these requirements aren't met.
		 * */

		public static List<SessionData> PROBLEM_SETS = new List<SessionData>{
			new SessionData(
				0,
				Activity.OPEN_CLOSED_SYLLABLE, 
				new string[]{"bet","dad","tin"}, //target words
				new string[]{"bt","dd","tn"} //placeholder letters initially placed
			),
			new SessionData(
				1,
				Activity.OPEN_CLOSED_SYLLABLE, 
				new string[]{"pup","hit","web"},
				new string[]{"pp","ht","wb"}
			),
			new SessionData(
				2,
				Activity.CONSONANT_BLENDS,
				new string[]{"flag","skin","stop"},
				new string[]{"ag","in","op"}
			),
			new SessionData(
				3,
				Activity.CONSONANT_BLENDS,
				new string[]{"trip","drop","crab"},
				new string[]{"ip","op","ab"}
			),
			new SessionData(
				4,
				Activity.CONSONANT_DIGRAPHS, 
				new string[]{"thin","shop","chip"},
				new string[]{"in","op","ip"}
			),
			new SessionData(
				5,
				Activity.CONSONANT_DIGRAPHS, 
				new string[]{"path","wish","lunch",},
				new string[]{"pa","wi","lun"}
			),
			new SessionData(
				6,
				Activity.MAGIC_E,
				new string[]{"game","tape","cake"},
				new string[]{"gm","tp","ck"}
			),
			new SessionData(
				7,
				Activity.MAGIC_E,
				new string[]{"side","wide","late"},
				new string[]{"sd","wd","lt"}
			),
			new SessionData(
				8,
				Activity.VOWEL_DIGRAPHS,
				new string[]{"eat","boat","paid"},
				new string[]{"t","bt","pd"}
			),
			new SessionData(
				9,
				Activity.VOWEL_DIGRAPHS,
				new string[]{"seat","coat","bait"},
				new string[]{"st","ct","bt"}
			),
			new SessionData(
				10,
				Activity.R_CONTROLLED_VOWELS,
				new string[]{"car","jar","fir"},
				new string[]{"c","j","f"}
			),
			new SessionData(
				11,
				Activity.R_CONTROLLED_VOWELS,
				new string[]{"hurt","horn","part"},
				new string[]{"ht","hn","pt"}
			),
		};

		public static int NUM_SESSIONS = PROBLEM_SETS.Count;

		public static Activity ActivityForSession(int session){
			if (session < NUM_SESSIONS)
				return PROBLEM_SETS [session].activity;
			throw new Exception($"Invalid session: {session}");
		}

		public static ProblemData GetProblem(int session, int problem){
			if (session < NUM_SESSIONS && problem < PROBLEMS_PER_SESSION) {
				return PROBLEM_SETS [session].problems [problem];
			}
			throw new Exception($"Either session ({session}) exceeds {NUM_SESSIONS} or problem ({problem}) exceeds {PROBLEMS_PER_SESSION}");
		}

	}

	public class Hints{
		public static readonly int NUM_HINTS = 3;
		public static readonly int LEVEL_2_SECONDS_DURATION_EACH_CORRECT_LETTER=2;
		public static readonly int LEVEL_2_SECONDS_DURATION_FULL_CORRECT_WORD=5;

		public class Descriptions{
			public const int PLAY_SOUNDED_OUT_TARGET_WORD = 0;
			public const int PRESENT_EACH_TARGET_LETTER_IN_SEQUENCE = 1;
			public const int PRESENT_TARGET_WORD_WITH_IMAGE_AND_FORCE_CORRECT_PLACEMENT = 2;
		}
	}

	public class Flash{

		public class Times
		{
			public static readonly int TIMES_TO_FLASH_ERRORNEOUS_LETTER = 1;
			public static readonly int TIMES_TO_FLASH_CORRECT_PORTION_OF_FINAL_GRAPHEME = 1;
			public static readonly int TIMES_TO_FLASH_ON_COMPLETE_TARGET_GRAPHEME = 3;

		}

		public class Durations{
			public static readonly float ERROR_OFF = .5f;
			public static readonly float ERROR_ON = .5f;
			public static readonly float HINT_TARGET_COLOR = 1f;
			public static readonly float HINT_OFF = .5f;
			public static readonly float CORRECT_TARGET_COLOR = 1f;
			public static readonly float CORRECT_OFF = .5f;

		}
	}


	public class SessionData{
		public int number;
		public Activity activity;
		public ProblemData[] problems;

		public SessionData(int number, Activity activity, string[] targetWords, string[] placeHolderLetters){
			this.number=number;
			this.activity = activity;

			if(placeHolderLetters.Length != targetWords.Length) throw new Exception("Error: number of target words must equal number of placeholder words");
			if(StudentMode.PROBLEMS_PER_SESSION == 0) StudentMode.PROBLEMS_PER_SESSION = placeHolderLetters.Length;
			else if(StudentMode.PROBLEMS_PER_SESSION !=  placeHolderLetters.Length) throw new Exception("Each session must have the same number of problems.");

			problems = new ProblemData[StudentMode.PROBLEMS_PER_SESSION];
			for(int i=0;i<problems.Length;i++){
				string targetWord = targetWords[i];
				string placeHolder = placeHolderLetters[i];
				problems[i]=new ProblemData(
					targetWord,
					placeHolder,
					new AudioClip[]
					{
						InstructionsAudio.instance.makeTheWord,
						AudioSourceController.GetWordFromResources (targetWord)
					}
				);

			}
		}
	}

}

public class ProblemData{
	public string targetWord;
	public string initialWord;
	public AudioClip[] instructions;
	public ProblemData(string targetWord, string initialWord, AudioClip[] instructions){
		this.targetWord = targetWord;
		this.initialWord = targetWord.Align(initialWord);
		this.instructions = instructions;
	}
		
}
