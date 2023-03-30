import mythic_container
import subprocess
from slack.c2_functions.slack import *

p = subprocess.Popen(["dotnet", "publish", "-c", "Release", "-o", "/Mythic/slack/c2_code/"], cwd="/Mythic/slack/c2_code/src/")
p.wait()

mythic_container.mythic_service.start_and_run_forever()