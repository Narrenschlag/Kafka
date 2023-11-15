# Kafka Dialogue System

`by Maximilian Schecklmann aka. Narrenschlag`

`Dependencies: Cutulu SDK`

#### Narrator

> **FollowType**
> 
> *Identifier for follow up interactions with the dialogue system. For example: The FollowType is set to close it will close the dialogue after the next interaction no matter what is defined. It is defined when the dialogue gets loaded.*

#### String Command

> `{local:jump()}`
> 
> `{global:loadMainMenu()}`
> 
> `{local:functionName(arg0, arg1)}`
> 
> `{global:function0(arg0), function1()}`

String command syntax is encapsuled by '{' '}' brackets. The key parameter defines the level of command. Global and local. Local commands will be executed first. Multiple commands can be executed by seperating the functions by a comma and they are all called by the same type.

#### String Enrichment

> `{set:string:playerName = Max}` 
> 
> `{mod:int:playerHealth = 5}` 
> 
> `{get:string:playerName}`

Espescially useful for chatting with the player and showing data without opening an extra window. This way characters can talk to the player with it's given name, can talk about the weather, make compliments about items and much more.
