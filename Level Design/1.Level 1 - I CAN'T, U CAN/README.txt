Level 1 - I CAN U CAN'T

Here the players would assume that the rocks need to be pushed, something they've experienced already. Lala will then push IS between ROCK and PUSH, so that Baba can now push rocks and get to the flag. The problem is, there's only one IS and it needs to be in the white FLAG_WIN statement. By pushing IS to ROCK_PUSH, you lock the IS and thus Baba's win condition, which is under Lala's control. 

The solution is to simply disable ROCK IS STOP, instead of enabling ROCK IS PUSH. (By breaking ROCK IS STOP statement)

Notes:
I dunno if this one is too tricky for a starting level, but I wanted to emphasize the ability to simply break statements, and not just rearrange them.