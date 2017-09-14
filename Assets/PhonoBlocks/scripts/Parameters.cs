
using UnityEngine;
using System;

public class Parameters : MonoBehaviour {

	[SerializeField] InputType inputType;

	void Start ()
	{     

		Events.Dispatcher.RecordInputTypeSelected (inputType);
		Events.Dispatcher.OnActivitySelected += (Activity obj) => {
			Application.LoadLevel ("Activity");
		};
	}

	public class StudentMode{
		
		public static Activity ActivityForSession(int session){
			switch (session) {
			case 0:
			case 1:
				return Activity.OPEN_CLOSED_SYLLABLE;

			case 2:
			case 3:
				return Activity.CONSONANT_BLENDS;

			case 4:
			case 5:
				return Activity.CONSONANT_DIGRAPHS;


			case 6:
			case 7:
				return Activity.MAGIC_E;

			case 8:
			case 9:
				return Activity.VOWEL_DIGRAPHS;
			case 10:
			case 11:

				return Activity.R_CONTROLLED_VOWELS;

			}
			throw new Exception($"Invalid session: {session}");

		}
		static string[][][] SESSION_WORD_SETS = {
			//session 1
			new string[][]{
				new string[]{"bet","dad","tin"}, //target words
				new string[]{"b t","d d","t n"} //placeholder letters initially placed
			},

			//session 2
			new string[][]{
				new string[]{"pup","hit","web"},
				new string[]{"p p","h t","w b"}
			},
			//session 3
			new string[][]{
				new string[]{"flag","skin","stop"},
				new string[]{"ag","in","op"}
			},
			//session 4
			new string[][]{
				new string[]{"trip","drop","crab"},
				new string[]{"ip","op","ab"}
			},
			//session 5
			new string[][]{
				new string[]{"thin","shop","chip"},
				new string[]{"in","op","ip"}
			},

			//session 6
			new string[][]{
				new string[]{"path","wish","lunch",},
				new string[]{"pa","wi","lun"}
			},
			//session 7
			new string[][]{
				new string[]{"game","tape","cake"},
				new string[]{"g m","t p","c k"}
			},
			//session 8
			new string[][]{
				new string[]{"side","wide","late"},
				new string[]{"s d","w d","l t"}
			},
			//session 9
			new string[][]{
				new string[]{"eat","boat","paid"},
				new string[]{"t","b  t","p  d"}
			},

			//session 10
			new string[][]{
				new string[]{"seat","coat","bait"},
				new string[]{"s  t","c  t","b  t"}
			}, 
			//session 1
			new string[][]{
				new string[]{"car","jar","fir"},
				new string[]{"c","j","f"}
			},
			//session 1
			new string[][]{
				new string[]{"hurt","horn","part"},
				new string[]{"h  t","h  n","p  t"}
			}
		};
		public static int NUM_SESSIONS = SESSION_WORD_SETS.Length;
		public static int PROBLEMS_PER_SESSION = SESSION_WORD_SETS[0][0].Length;


		public static string PlaceholderLettersFor(int session, int problem){
			if (session < NUM_SESSIONS && problem < PROBLEMS_PER_SESSION) {
				return SESSION_WORD_SETS [session][1] [problem];
			}
			throw new Exception($"Either session ({session}) exceeds {NUM_SESSIONS} or problem ({problem}) exceeds {PROBLEMS_PER_SESSION}");

		}
		public static string TargetWordFor(int session, int problem){
			if (session < SESSION_WORD_SETS.Length && problem < PROBLEMS_PER_SESSION) {
				return SESSION_WORD_SETS [session][0] [problem];
			}
			throw new Exception($"Either session ({session}) exceeds {NUM_SESSIONS} or problem ({problem}) exceeds {PROBLEMS_PER_SESSION}");
		}


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

}
