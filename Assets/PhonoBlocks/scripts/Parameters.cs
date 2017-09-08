//todo make globally accessible state and event files that ar ealso organized in this way
//only do this when you have finished accomplishing all todos and cleaning up dependencies as best we can
using UnityEngine;

public class Parameters {


	/*
	 * 
	 * green = Color.green;
				green.r = (float)95 / (float)255;
				green.b = (float)127 / (float)255;
				green.g = (float)180 / (float)255;
				blue = Color.blue;
				blue.r = (float)105 / (float)255;
				blue.g = (float)210 / (float)255;
				blue.b = (float)231 / (float)255;
				red = Color.red;
				red.g = (float)58 / (float)255;
				red.b = (float)68 / (float)255;
				pink = Color.red;
				pink.r = (float)247 / (float)255;
				pink.g = (float)98 / (float)255;
				pink.b = (float)162 / (float)255;
				yellow = Color.yellow;
				yellow.r = (float)249 / (float)255;
				yellow.g = (float)249 / (float)255;
				yellow.b = (float)98 / (float)255;
				gray = Color.gray;
				gray.r = (float)(gray.r * 1.2);
				gray.g = gray.r;
				gray.b = gray.r;
	 * 
	 * 
	 * */

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

		}

		public class ConsonantBlendColors{
			public static Color COMPLETED_BLEND_COLOR = green;
			public static Color SINGLE_CONSONANT_COLOR = blue;

		}

		public class VowelDigraphColors{
			public static Color COMPLETED_DIGRAPH_COLOR = red;

		}

		public class RControlledVowelColors{
			public static Color R_CONTROLLED_VOWEL_COLOR = red;
			public static Color R_CONTROLLED_R_COLOR = red;
		}

	}

	public class Hints{
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
