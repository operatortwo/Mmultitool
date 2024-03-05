# Mmultitool
**Midi Multi Tool** is intended to be a library that will provide various functions related to MIDI data. Instead of working on similar functions in several projects, parts of this work are brought together here.  
The library is written for .Net Framework and the preferred application type is a WPF app (.Net Framework)




## Content

### Midifile Reader

### Event Lister

### Using from WinForms
The library can also be used by a WinForms application, although there may be restrictions when using the controls.  
For example, in a WPF Application, EventLister's DataGridRowBackground property appears in the VisualStudio Properties window and can be edited there, 
while in a WinForms application the EventLister control is hidden behind the ElementHost and the properties are only accessible via code.
