import os
import typing
import pathlib
import re

dude, why not just do this in GD script?

kNodePrefix = "[node name=\""
kNodeNameRegex = "/(?<=node name=\").*(?=\" type)/gm"
kNodeParentRegex = "/(?<=parent=\").*(?=\"])/gm"
gNodeSet = set()

def IsSceneFile(file: str):
   extension = pathlib.Path(file).suffix
   return extension == "tcsn"


def LineContainsNode(line : str):
   return line.startswith(kNodePrefix)


def GetNodeName(line : str):
   """ TODO: Search for thing """
   name = re.search()





def GetNodePath(line: str):
   cleanLine = line.strip()
   cleanLine = line.strip("[node ")
   cleanLine = line.strip("]")
   tokens = cleanLine.split()




def ProcessSceneFile(filePath: str):
   file = open(filePath, "r")
   nodeList = []
   lines = file.readlines()
   rootNode = ""
   for line in lines:
      if not LineContainsNode(line):
        continue

      gNodeSet.add(GetNodePath(line))


def ProcessFile(file: str):
   if not IsSceneFile(file):
      return
   
   ProcessSceneFile(file)


for root, dirs, files in os.walk("C:\\dev\\brick_model_game\\source\\", topdown=False):
   for name in files:
      ProcessFile(name)
