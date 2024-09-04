# FBLA-2024-2025
This is the repo between Logan Barber, Eric Hill, and Linh Le for the 2024 - 2025 FBLA Computer Game and Simulation Event.

## Discord
https://discord.gg/5fErcgS7

## Installation
![image](https://github.com/user-attachments/assets/562848c3-8b47-4b20-9f6e-f16ccb84dc32)

First download git using this link: https://git-scm.com/downloads
Run the installer and just skip through the dialogue, we aren't going in depth enough for most of it to matter.

Once installed navigate to the folder position you want and right click, then click "open in termainal"
![image](https://github.com/user-attachments/assets/58ae20a7-6eb6-437b-8970-0d5e3186e79c)

If you aren't in the location you want navigate their in windows powershell or command prompt via "cd" commands.
<I> For more information visit </I> https://static1.howtogeekimages.com/wordpress/wp-content/uploads/2022/09/captures-click-open-in-termina.png

Then run the git clone command which is run by typing "git clone https://github.com/DarthCooper/FBLA-2024-2025.git"

You should now have the game files on your computer, but before you get ahead of yourself you have to sign in.
This can be done by running <I> two </I> commands.

git config --global user.email "you@example.com" {Replace every word between the quotation marks with your email}
git config --global user.name "Your Name" {Replace every word between the quotation marks with your github username}

### Using Github Desktop
![image](https://github.com/user-attachments/assets/4f27804b-6b30-4fc5-ba90-21f934550f13)

I'd explain it here, but it's already written out so here ya go:
https://docs.github.com/en/desktop/adding-and-cloning-repositories/cloning-a-repository-from-github-to-github-desktop

## Pulling
In order to sync the changes made by other people you have to "pull" the repo. This can be done in a couple way.

### Using Git
If you already have Git installed then use this, if not keep scrolling.
you'll have to navigate to the repo again using the "cd" commands, but once you are there you can run the command.
The command is "git pull" or git pull origin "exampleBranch" {again replace everything between the quotation marks with the branch name}

git pull docs - https://git-scm.com/docs/git-pull/en

### Using Github Desktop
again it's been written out by github themself so here ya go:
https://docs.github.com/en/desktop/working-with-your-remote-repository-on-github-or-github-enterprise/syncing-your-branch-in-github-desktop

## Pushing
In order to sync your changes to the server you have to "push" to the repo. This can be done in a variety of ways, but I'm only gonna show two.
![image](https://github.com/user-attachments/assets/c24d3e1b-452e-448a-a22f-2a99ac3abda3)


### Using Git
If you already have Git installed then use this, if not keep scrolling.
you'll have to navigate to the repo again using the "cd" commands, but once you're in there you can run the commands.

If you made a new file you have to run "git add \file location + file name\ {obviously you have to fill the file location and file name in} It should look like "git add .\Assets\"
If it's saying that you can't add it because the gitignore is blocking it then you better not add it.

If you made changes to files then you can sync those files via "git stage -p". This will then bring up a lot of prompts asking if you want to stage these code pieces. Type 'y' for yes and 'n' for no.

Once you have staged and/or added you can commit your changes. This is done by running the command "git commit -m "the commits description" {You'll want to put in a valid descriptiion of the changes being done}

Now that your changes are commited you can run "git push" to push to your local branch, or "git push origin {branch name}"

### Using Github Desktop
You'll be able to see your changes on the left, and you can select which items to add by changing the checkmark by each item or the checkmark at the top.
You'll then be able to add a commit name, which should reflect what changes you made.
Then you can click the push button in the upper right corner.

docs - https://docs.github.com/en/desktop/making-changes-in-a-branch/pushing-changes-to-github-from-github-desktop

## Connecting to Unity
![image](https://github.com/user-attachments/assets/c92b4b96-08cb-4a7b-b11d-452c8e38b649)

Download Unity Hub from this link https://unity.com/download

Run the installer and lanuch Unity Hub once it's done.

From there click install editor and install whatever editor I specified. We'll try and stay with Unity 6 for now.

Once the editor is installed, return to unity hub and click the arrow next to add and then "add project from disk". Then navigate to the repo.
Wait for it to launch as it may take a bit, but once it's done you'll have the game on unity.
