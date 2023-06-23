# ❄️ Over-Ice
![bannersnow](https://user-images.githubusercontent.com/73078691/209925383-f8fe4f71-a96a-4685-b735-25386bae0e1a.png)
##
### 🧊 What is Over-Ice?
Over-Ice is an advanced workshop-script compiler and decompiler, allowing you to code your own gamemodes outside of Overwatch, whenever and wherever.
### ❓ Why Over-Ice?
Since Overwatch 2's release, there is no way to access workshop except with a text editor. So why struggle with notepad? Open up an IDE and improve your worktime and workflow.
# 📘 What does Over-Ice have to offer?
![secondbanner](https://user-images.githubusercontent.com/73078691/209931952-e11d7976-fea9-4f32-b2a8-5c9b49ad0ba3.png)
Over-Ice offers:
### ``Operator Trees``
> No more unnecessary and complicated add(1, 2), it's "1+2" now.
### ``Optional Argument Calling`` 
> Are you tired of adding a bunch of nulls when creating a hud text? Well now you can call the optional arguments in actions, like so:
```
hud(local, subheader: "SubHeader Text", color: Color::White);
```
### ``Over the top optimization``
> Over-Ice's optimization uses basic, small optimizations, such as turning 0's into Empty Arrays. Where it truly shines, is in context.
> Take as example, the following action:
```
x = 1 + 2 + 3;
```
> Over-Ice will pre-calculate the value, so the output will become:
``
x = 6;
``
> This is just the tip of the iceberg however, it'll also remove redundancies, such as a condition being 
``
HeroOf(local) == Hero::Ana
``
> When it's already specified that local will be Ana, in event. Therefore, it'll be removed. In actions, it'll be translated into just
``
Hero::Ana
``
> In the offchance it doesn't produce the expected outcome, the ``unoptimize`` keyword can be used. Like so:
```
unoptimize 
{
  x = 1 + 2 + 3;
};
```
### ``Macros``
### ``Switches``
### ``Enums``
### ``Type defined variables``
### ``INIT Rules``
> In the case a variable is defined like so: ``global float x = 5``, Over-Ice will create an Initial Rule, where it'll set it's value there. However, if you have your own
Initial Rules, you can use the keyword ``override`` like so:
```
override init() -> global()
{
  ...
}
```
### ``Importing Files``
> Over-Ice has two ways of importing files.
``#import "File.ovice"``
and
``#import <File.ovice>``.
The difference between the two is that, files enclosed in ``<>`` will be searched in a specific directory, therefore you can place all of your most used files there, 
and not worry about having to copy it to your project directory. Files enclosed in ``""`` will be searched in the path provided, the path start being the current folder.

# 🌨️ Wiki
![Untitled-5](https://user-images.githubusercontent.com/73078691/209933480-95e2e25b-92c2-48f3-a401-56935368c013.png)
> Can't figure something out?
[Check out the wiki!](https://github.com/Preciseful/Over-Ice/wiki)
