# Mmultitool
**Midi Multi Tool** is intended to be a library that will provide various functions related to MIDI data. Instead of working on similar functions in several projects, parts of this work are brought together here.  
The library is written for .Net Framework and the preferred application type is a WPF app (.Net Framework)



## Content

### Midifile Reader
Reads midi files (.mid), based on the Midifile repository.

### Event Lister
Shows events in a DataGrid. The events can be filtered by track, channel and event type.  
Time can be displayed as Ticks or as Measure:Beat:Tick. Status can be displayed as Hex or Decimal.  
The selected events can be copied to the Clipboard using **right-click**. From there, the tab-delimited text can be pasted into a text application or a spreadsheet.  

![EventLister](https://github.com/operatortwo/Mmultitool/assets/88147904/0fed225d-97e5-405e-abbd-e15e66b7ce6a)



---
### Using from WinForms
The library can also be used by a WinForms application, although there may be restrictions when using the controls.  
For example, in a WPF Application, EventLister's DataGridRowBackground property appears in the VisualStudio Properties window and can be edited there, 
while in a WinForms application the EventLister control is hidden behind the ElementHost and the properties are only accessible via code.  

```
EventLister1.DataGridRowBackground = Brushes.AliceBlue
```

