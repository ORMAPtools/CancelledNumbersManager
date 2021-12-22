# ORMAP - Cancelled Numbers Manager Add-in (for ArcGIS Pro)

The ORMAP Cancelled Numbers Manager is an Add-in for ArcGIS Pro that helps managing cancelled numbers within the ArcGIS Pro software.  It allows users to manually add, delete, or move (rearrange) one or more cancelled number items.  Cancelled numbers are stored in the ORMAP data schema table named "CancelledNumbers".  This tool is an interface which allows easy and quick interaction with the items in that table. 


![alt text](https://raw.githubusercontent.com/ORMAPtools/CancelledNumbersManager/main/Images/Toolbar1.PNG "Image of the toolbar")


![alt text](https://raw.githubusercontent.com/ORMAPtools/CancelledNumbersManager/main/Images/Toolbar2.PNG "Image of the toolbar")

## Download
The download contains the Add-in. Additional information about installation and requirments are below.
> [DOWNLOAD NOW](https://raw.githubusercontent.com/ORMAPtools/CancelledNumbersManager/master/Install/ORMAPCancelledNumbers.esriAddinX).


## Installation
Installation of add-ins can be found HERE:
> [Manage add-ins](https://raw.githubusercontent.com/ORMAPtools/CancelledNumbersManager/master/Install/ORMAPCancelledNumbers.esriAddinX).

This Add-In has a few requirements.
1.	A standalone table named "CancelledNumbers" must exist in the table of contents (the database name does not matter, only the name in the table of contents).  
2.	The following fields must be present within the CancelledNumbers table: MapNumber (Text), Taxlot (Text), SortOrder (Short). 

### Usage
1.	Start typing the MapNumber for the cancelled numbers you want to edit.  
2.	After 4 characters are entred, a dropdown will appear for you to choose from, keep typing, use the up/down arrow keys, or select the ManNumber from the list. 
3.	Select one or more Taxlots from the list to move or delete.   
4.	Use the buttons to move or delete the selected taxlots. 
5.	To add a new taxlot (or Text) enter a new taxlot in the input box at the bottom of the table and press Add.   
6.	Save changes by clicking the Update button. 


### Issues
Find a bug or want to request a new feature?  Please let us know by submitting an [issue](https://github.com/ORMAPtools/CancelledNumbersManager/issues). 

Having problems with the toolbar? Check out the issue tracker to see if your [problem](https://github.com/ORMAPtools/CancelledNumbersManager/issues) has already been resolved by someone else.

### Credit/Contributions
The ORMAP tools were created by the ORMAP tools developers.  We encourage anyone to help contribute to the ORMAP tools project.  Please submit an [issue](https://github.com/ORMAPtools/CancelledNumbersManager/issues) or fork and create a pull request.


### Licensing
Licensed under the GNU General Public License, version 3 (GPL-3.0).  
> [View License](https://github.com/ORMAPtools/CancelledNumbersManager/blob/main/LICENSE).