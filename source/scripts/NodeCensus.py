import os
import typing
import pathlib
import re


kNodePrefix = "[node name=\""
kNodeNameRegex = "/(?<=node name=\").*(?=\" type)/gm"
kNodeParentRegex = "/(?<=parent=\").*(?=\"])/gm"


def IsSceneFile(file: str):
   extension = pathlib.Path(file).suffix
   return extension == "tcsn"


def LineContainsNode(line : str):
   return line.startswith(kNodePrefix)

def GetNodeName(line : str)
   name = re.search()

def ProcessSceneFile(filePath: str):
   file = open(filePath, "r")
   nodeList = []
   lines = file.readlines()
   rootNode = ""
   for line in lines:
      if not LineContainsNode(line):
        continue


def ProcessFile(file: str):
   if not IsSceneFile(file):
      return
   
   ProcessSceneFile(file)


for root, dirs, files in os.walk("C:\\dev\\brick_model_game\\source\\", topdown=False):
   for name in files:
      ProcessFile(name)
