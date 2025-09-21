import warnings
warnings.filterwarnings("ignore")

from crewai import Agent, Task, Crew
import os
try:
	from utils import get_openai_api_key
except ImportError:
	def get_openai_api_key():
		raise ImportError("utils.py with get_openai_api_key() not found. Please add it to your project.")

openai_api_key = get_openai_api_key()
os.environ["OPENAI_API_KEY"] = openai_api_key
os.environ["OPENAI_MODEL_NAME"] = 'gpt-4'

planner= Agent(
	role="Blog content planner",
	goal="Plan engaging and factually accurate content on {topic} for a compelling story-telling through blog contents",
	backstory=("You are working on planning a blog article "
    "about the topic: {topic}. "
	"You collect information, organize it in a well structured format that helps audience to consume the information easily and audience learn something "
	"and make informed decisions. "
	"Your work is the basis for the content writer to write an article / blog post on this topic"),
	allow_delegation=False,
	verbose=True
	)
writer = Agent(
	role="Blog content writer",
	goal="Write insightful and factually accurate, opinion piece, in a compelling story-telling format about the {topic}.",
	backstory=("You are working on writing "
	 "a new opinion piece about the topic: {topic}. "
	 "You base your writing on the work of "
	 "the Content Planner, who provides an outline "
	 "and relevant context about the topic. "
	 "You follow the main objectives and direction of the outline, "
	 "as provided by the Content Planner. "
	 "You also provide objective and impartial insights "
	 "and back them up with the information provided by the Content Planner. "
	 "When your statements are opinions, as opposed to objective statements, "
	 "make that clear to the reader."),
	allow_delegation=False,
	verbose=True
)

editor = Agent(
    role="Blog content editor",
    goal="Edit the blog content for clarity, coherence, grammar, and factual accuracy. Ensure the content flows well and is engaging for the target audience.",
    backstory="You are working on editing a blog article about the topic: {topic}.",
    allow_delegation=False,
    verbose=True
)

plan= Task(
	name="Plan the blog content",       
	description=(
	"Create a detailed outline for a blog article on the topic: {topic}. with a call to action"
				   "Priortize the latest trends, key players and noteworthy news on {topic}."
				   "Idenitfy and address the audience pain points and hook them on {topic}."
				   "include SEO keywords and relevant data or sources."
),
	expected_output="A comprehensive content plan document with an outline, audience analysis,."
	                "SEO keywords, and resources",
	
	agent=planner
)

write= Task(
    name="Write the blog content",
    description="Write the blog content based on the outline created in the planning phase.",
    agent=writer,
    context=[plan],  # Add dependency on plan task
    expected_output="A comprehensive blog article based on the plan provided"
)
edit= Task(
    name="Edit the blog content",
    description="Edit the blog content for clarity, coherence, grammar, and factual accuracy.",
    agent=editor,
    context=[write],  # Add dependency on write task
    expected_output="A polished and refined blog article ready for publication."
)   

crew= Crew(
	agents=[planner, writer, editor],
	tasks=[plan, write, edit],
	verbose=True
)

result= crew.kickoff(inputs={"topic":"AI Engineering and AI-first Engineering approach"})