//todo make globally accessible state and event files that ar ealso organized in this way
//only do this when you have finished accomplishing all todos and cleaning up dependencies as best we can
public class Parameters {

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
