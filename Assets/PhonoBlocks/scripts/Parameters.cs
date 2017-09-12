//todo make globally accessible state and event files that ar ealso organized in this way
//only do this when you have finished accomplishing all todos and cleaning up dependencies as best we can
using UnityEngine;

public class Parameters {

	public class UI{
		public static readonly int ONSCREEN_LETTER_SPACES = 6;
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
