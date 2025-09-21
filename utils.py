import os
from getpass import getpass
from dotenv import load_dotenv

def get_openai_api_key():
    """
    Get the OpenAI API key from environment variable or prompt user for input.
    
    Returns:
        str: The OpenAI API key
    """
    # Load environment variables from .env file
    load_dotenv()
    
    api_key = os.environ.get("OPENAI_API_KEY")
    
    if not api_key:
        # If not in environment variables, prompt the user
        print("OpenAI API key not found in environment variables.")
        api_key = getpass("Please enter your OpenAI API key: ")
        
        # Save to environment variable for current session
        os.environ["OPENAI_API_KEY"] = api_key
    
    return api_key