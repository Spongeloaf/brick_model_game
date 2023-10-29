import os
import typing
import pathlib
import re
from treelib import Node, Tree
import glob

# dude, why not just do this in GD script?
# because I hate GD script. There's no way to do it that doesn't involve
# strange get_node() functions that never return what I expect.
# Plus doing it outside GDscript means we don't have to instaniate scenes
# Also the IDE and debugger for python is just better than GDscript in
# the godot editor or in VSCode.

# I'm a curmudgeon. Make it yourself in GD script if you feel that stroingly about it.

# To continue work on this: Just download that python tree lib and go for it.

kNodePrefix = "[node name=\""
kNodeNameRegex = "/(?<=node name=\").*(?=\" type)/gm"
kNodeParentRegex = "/(?<=parent=\").*(?=\"])/gm"
kFileList = glob.glob("C:\\dev\\brick_model_game\\source\\**\*", recursive=True)
kTree = Tree()


def IsSceneFile(file: str):
   extension = pathlib.Path(file).suffix
   return extension == ".tscn"


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

      pass


def ProcessFile(file: str):
   if not IsSceneFile(file):
      return
   
   ProcessSceneFile(file)


# def AddNodeToTree(string NodePath):
#    kTree.create_node()



for file in kFileList:
      ProcessFile(file)
pass
