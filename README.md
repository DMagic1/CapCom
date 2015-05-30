### **CapCom - Mission Control On The Go**
[![][shield:support-ksp]][KSP:developers]&nbsp;
[![][shield:ckan]][CKAN:org]&nbsp;
[![][shield:license-mit]][CCLicense]&nbsp;
[![][shield:license-cc-by-sa]][CCLicense]&nbsp;

![][CC:header]

[![][shield:support-toolbar]][toolbar:release]&nbsp;
[![][shield:support-cwp]][cwp:release]&nbsp;


### People, and Info
-------------------------------------------

#### Authors and Contributors

[DMagic][DMagic]: Author and maintainer

[TriggerAu][TriggerAu]: CapCom uses a modified version of TriggerAu's KSP Plugin Framework

#### License

The code is released under the [MIT license][CCLicense]; all art assets are released under the [CC-BY-SA 
license][CCLicense]

#### FAQ

 * What is CapCom?
     * CapCom is a user interface that allows for reviewing, accepting, declining or canceling contracts outside of the Mission Control Center building.
 * How do the keyboard shortcuts work?
     * When the window is selected keyboard shortcuts are available for several functions
	 * The **Up/Down arrows** are used to cycle through contracts in the currently selected list
	 * The **Right/Left arrows** are used to switch lists
	 * The **Enter** key is used to accept an offered contract
	 * The **Del** key is used to decline or cancel a contract
	 * All keys can be reassigned in the settings window
 * Is CapCom designed to replace the in-flight contract list app?
     * CapCom can be used to monitor active contracts, but it is not designed as a replacement for the Contracts App.
     * For an addon designed specifically for doing that try the [Contracts Window +][cwp:release].
 * Something has gone wrong; where can I get help?
     * If you run into errors, contracts not visible in a list, contracts not being accepted, window not being displayed properly, etc... report problems either to the [GitHub Issues][CC:issues] section or the [KSP Forum Thread][CC:release]. 

### 
-------------------------------


#### Sort Bar
![][CC:sort-bar]

##### Controls contract sorting options and order
  * Contracts can be sorted based on a variety of criteria in either ascending or descending order
       * **Contract difficulty**, as noted by the number of stars	
       * **Contract reward** amounts; **Funds**, **Science**, or **Reputation**
       * **Contract agency**, sorted alphabetically
       * **Target Planet**, not all contracts have target planets associated with them; addon contracts rely on the planet being in the contract title
  * Current active contract count and max contract limit are shown above 
  
##### Selecting the rewards sorting option will open a drop-down menu; select the desired reward type here
![][CC:sort-rewards]

#### Contract List
![][CC:contract-list]

##### Separate contract lists are available based on contract status
  * Review, accept, and decline **offered contracts**
       * Dependent upon the maximum contracts allowed by the current Mission Control Center building level
  * Review **active contracts**
       * Mission status is updated for each contract
	   * Cancel active contracts if allowed
  * Review **completed contracts**

#### Contract Header
![][CC:contract-header]

##### The title and agency of the currently selected contract is shown here, along with the primary contract controls
  * The **accept**, **decline**, and **cancel** buttons are shown on the right, depending on the contract's status
  * The **agency flag** is a button, pressing it opens the agency info screen
  * The **settings menu** can be opened with the gear icon in the upper-right
  * The *X* icon will close the window

##### By default a **warning popup** is displayed when you decline or cancel a contract; both options can be adjusted in the settings menu.
![][CC:contract-warn]  
  
#### Basic Contract Info
![][CC:contract-info]

##### The nonsense **mission briefing** and short mission synopsis are shown at the top of this area
  * An option to hide the **mission briefing** text is available in the settings menu
  * **Mission duration** and **deadline** times are updated in real-time
  
#### Contract Objectives
![][CC:objectives]

##### Each **contract parameter**, its status and any rewards are displayed next
  * The **status** of each parameter is indicated by an icon on the left
  * Parameter and mission **notes** can be hidden by default using an option in the settings menu; if selected they can then be displayed by clicking a blue **note icon**
  * **Reward values** also incorporate any strategy modifiers 
  
#### Contract Rewards
![][CC:rewards]

##### The overall **mission rewards** are displayed at the bottom of the window
  * These also reflect any changes made by **strategy modifiers**

#### Agency Info
![][CC:agency]

##### When the agency flag is selected a separate text area will appear
  * Basic information about the current agency is displayed
  * Each agency mentality is shown, along with a description if available
  * Other contracts offered or already accepted by the same agency are displayed  

------------------------------
  
### Settings Window
![][CC:settings-full]

#### Config Options
![][CC:settings-options]

##### At the top of the **settings window** are several toggle options
   * **Hide Mission Briefing Text** will prevent the nonsense mission briefing from being shown at all
   * **Hide Mission Notes** will cause mission and parameter notes to be hidden by default, they can be displayed by clicking on the blue **+** icon next to each note
   * **Warn on Decline** causes a warning window with a confirmation button to appear when declining an offered contract
   * **Warn on Cancel** causes a warning window with a confirmation button to appear when canceling an active contract
   * **Tooltips** are available for several of the icon buttons
   * **Use Stock App Launcher** is available only if [Blizzy78's Toolbar][toolbar:release] is installed; turning this option off will cause the CapCom button to use that toolbar
   
#### Keyboard Shortcuts
![][CC:settings-keys]

##### All of the keyboard shortcut keys can be reassigned here
   * Select the key to be reassigned using the buttons on the right
   * With the **reassign** window open push any key (don't take screenshots with the window open...); push accept to save the new setting
   * Use the **Save** button at the bottom to accept any changes; the **Cancel** button will revert any changes made
   

[DMagic]: http://forum.kerbalspaceprogram.com/members/59127
[TriggerAu]: http://forum.kerbalspaceprogram.com/members/59550

[KSP:developers]: https://kerbalspaceprogram.com/index.php
[CKAN:org]: http://ksp-ckan.org/
[CCLicense]: https://github.com/DMagic1/CapCom/blob/master/LICENSE

[CC:header]: http://i.imgur.com/uIUDjTi.png
[CC:settings-full]: http://i.imgur.com/qiEHuI7.png
[CC:sort-bar]: http://i.imgur.com/jpCV6Pe.png
[CC:sort-rewards]: http://i.imgur.com/p1NNhwy.png
[CC:contract-list]: http://i.imgur.com/6JCjutL.png
[CC:contract-header]: http://i.imgur.com/238zNhQ.png
[CC:contract-info]: http://i.imgur.com/FSbIHBq.png
[CC:contract-warn]: http://i.imgur.com/MYFREr2.png
[CC:rewards]: http://i.imgur.com/CIqqugF.png
[CC:objectives]: http://i.imgur.com/vheC0BB.png
[CC:agency]: http://i.imgur.com/taMAEqY.png
[CC:settings-options]: http://i.imgur.com/jDLiA9Q.png
[CC:settings-keys]: http://i.imgur.com/wB3wP1c.png

[CC:issues]: https://github.com/DMagic1/CapCom/issues
[CC:release]: http://forum.kerbalspaceprogram.com/threads/119701

[toolbar:release]: http://forum.kerbalspaceprogram.com/threads/60863
[cwp:release]: http://forum.kerbalspaceprogram.com/threads/91034

[shield:license-mit]: http://img.shields.io/badge/license-mit-a31f34.svg
[shield:license-cc-by-sa]: http://img.shields.io/badge/license-CC%20BY--SA-green.svg
[shield:support-ksp]: http://img.shields.io/badge/for%20KSP-v1.0.2-bad455.svg
[shield:ckan]: https://img.shields.io/badge/CKAN-Indexed-brightgreen.svg
[shield:support-toolbar]: http://img.shields.io/badge/works%20with%20Blizzy's%20Toolbar-1.7.9-7c69c0.svg
[shield:support-cwp]: https://img.shields.io/badge/works%20with%20Contracts%20Window%20%2B-5.1-orange.svg
