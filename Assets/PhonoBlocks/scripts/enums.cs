public enum Mode{TEACHER, STUDENT};

public enum Activity{
	MAGIC_E, 
	CONSONANT_DIGRAPHS, 
	CONSONANT_BLENDS, 
	VOWEL_DIGRAPHS, 
	R_CONTROLLED_VOWELS,
	OPEN_CLOSED_SYLLABLE
};

//states that the student activity can have. 
//by default, state for teacher mode is always main activity.
public enum ActivityStates{
	MAIN_ACTIVITY, //standard functionality (accept user input; update UI; update colors/error feedback)
	FORCE_CORRECT_LETTER_PLACEMENT, //don't update the GUI letter images unless the inputted letter is correct. still show error feedback.
	REMOVE_ALL_LETTERS //after the problem
}


public enum InputMode{
	KEYBOARD, TUI
}