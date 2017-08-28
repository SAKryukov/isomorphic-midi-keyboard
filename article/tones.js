const util = require("util");

const western = 12;
const cents = 1200;
const arabic = 2*12; // gadwal // https://en.wikipedia.org/wiki/Quarter_tone#Quarter_tone_scale

const cent = Math.pow(2, 1/cents);
const halftone = Math.pow(2, 1/western);
const quotertone = Math.pow(2, 1/arabic);

const notes = [ // https://en.wikipedia.org/wiki/Just_intonation
    {name: 'C', interval: 00, arabicInterval: 00, justNumerator: 01, justDenominator: 01},
    {name: 'D', interval: 02, arabicInterval: 04, justNumerator: 09, justDenominator: 08},
    {name: 'E', interval: 04, arabicInterval: 08, justNumerator: 05, justDenominator: 04},
    {name: 'F', interval: 05, arabicInterval: 10, justNumerator: 04, justDenominator: 03},
    {name: 'G', interval: 07, arabicInterval: 14, justNumerator: 03, justDenominator: 02},
    {name: 'A', interval: 09, arabicInterval: 18, justNumerator: 05, justDenominator: 03},
    {name: 'B', interval: 11, arabicInterval: 22, justNumerator: 15, justDenominator: 08}
];

for (let note of notes) {
    const equalFrequency = Math.pow(halftone, note.interval);
    const equalFrequencyInCents = Math.log(equalFrequency)/Math.log(cent);
    const justFrequency =  note.justNumerator / note.justDenominator;
    const justFrequencyInCents = Math.log(justFrequency)/Math.log(cent);
    const error = justFrequencyInCents - equalFrequencyInCents; 
    console.log(util.format("%s: error: %d (%d vs %d)", note.name, error, equalFrequency, justFrequency));
}
