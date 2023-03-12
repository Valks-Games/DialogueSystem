# Dialogue System

![Preview3](https://user-images.githubusercontent.com/6277739/224518350-491ff4a9-d026-4c52-a462-ca3780750914.PNG)

## Setup
1. Install [Godot 4 Mono Beta 16](https://downloads.tuxfamily.org/godotengine/4.0/beta16/mono/)

## How to use
Create a folder called "Dialogues" in root project

In that folder created a new txt called "fairy_encounter_1.txt"

In the txt add the following.

```
Valk: Hello!
Gerbo: Hi

*choice1: Fine
*choice2: Alright
*choice3: Terrible

[choice1]
Gerbo: That's great!
Gerbo: :D
[end]

[choice2]
Gerbo: Cool cool
[end]

[choice3]
Gerbo: .-.
[end]

Gerbo: alright well bye!
Valk: bye!
```

## Notes
- There is a bug that skips some dialogue. Have not figured out how to solve it yet.
